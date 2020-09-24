using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

[System.Serializable]
public struct CannonBallPacketData
{
    public int ID;
    public int ownerID;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public struct MinePacketData
{
    public int ID;
    public Vector3 position;
}

[System.Serializable]
public struct ShipPacketData
{
    public int ID;
    public Vector3 position;
    public Quaternion rotation;
    public int health;
    public float fuel;
}

[System.Serializable]
public struct ServerDataPacket
{
    public long ID;
    public bool fullUpdate;
    public string gameSession;

    public ShipPacketData player;

    public List<ShipPacketData> enemyShips;
    public List<MinePacketData> mines;
    public List<CannonBallPacketData> cannonBalls;
}

public class NetworkManager : MonoBehaviour
{
    //NetværksManager v1.1 - Corona Edition! (Credits: Oliver Rasmussen)

    // ----- Essentielle variabler ----- \\

    [SerializeField] GameManager gameManager = null;

    private readonly System.Uri gameserverUrl = new System.Uri("https://example.com/gameserver.php");
    private readonly HttpClient networkClient = new HttpClient();

    private bool isInitialized = false;
    [SerializeField] private bool isGameMaster = true;

    ///<summary>Hvor mange gange skal vi opdatere spillet per sekundt</summary>
    [SerializeField] private int updateInterval = 60;

    ///<summary>Logikken for rent faktisk og opdatere</summary>
    private float updateIntervalActual = 0.0f;
    private float updateIntervalNext = 0.0f;
    private float updateIntervalCurrent = 0.0f;
    private string updateIntervalSession = string.Empty;
    private ServerDataPacket updateIntervalPacket = new ServerDataPacket
    {
        ID = 0
    };

    // ----- Custom funktioner ----- \\

    ///<summary>Håndtere at få opdateret på de rigtige tidspunkter</summary>
    public void OnUpdate()
    {
        updateIntervalCurrent += Time.deltaTime;

        if (updateIntervalCurrent >= updateIntervalNext)
        {
            updateIntervalNext += updateIntervalActual;

            OnNetworkUpdate();
        }
    }

    ///<summary>Bliver kaldt når vi skal lave en netværks update</summary>
    private async void OnNetworkUpdate()
    {
        if (isGameMaster == true)
        {
            ServerDataPacket oldPacket = updateIntervalPacket;

            updateIntervalPacket = GenerateServerPacket(oldPacket);

            //Konverter data packeten til json format
            string packetDataJson = JsonUtility.ToJson(updateIntervalPacket);

            //Encode json stringen til base64 string
            string packetDataEncoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(packetDataJson));

            //Send vores data til serveren
            FormUrlEncodedContent packetValue = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("clientType", "master"),
                new KeyValuePair<string, string>("packetData", packetDataEncoded)
            });

            await networkClient.PostAsync(gameserverUrl, packetValue);
        }
        else
        {
            //Send en request til serveren om data
            FormUrlEncodedContent packetValue = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("clientType", "client")
            });

            HttpResponseMessage response = await networkClient.PostAsync(gameserverUrl, packetValue);
            string responseData = await response.Content.ReadAsStringAsync();

            //Tjek om der er data og vidersend til handleren
            if (responseData.Trim() != string.Empty)
            {
                //base 64 stringen til json string
                string packetDataDecoded = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(responseData));

                //Konverter json stringen til en data packet
                ServerDataPacket packetData = JsonUtility.FromJson<ServerDataPacket>(packetDataDecoded);

                //Vi tjekker her om sessionen har ændret sig eller om der er en fullupdate, hvis ja så fjerner vi alt i scenen og instantiere alt på nyt
                if(packetData.fullUpdate == true || packetData.gameSession != updateIntervalSession)
                {
                    updateIntervalSession = packetData.gameSession;

                    ResetNetworkedData();
                    CleanupUnnetworkedData();

                    OnNetworkDataReceived(packetData);

                    updateIntervalPacket = packetData;
                }
                else if(packetData.ID > updateIntervalPacket.ID)
                {
                    OnNetworkDataReceived(packetData);

                    updateIntervalPacket = packetData;
                }
            }
        }
    }

    ///<summary>Bliver kaldt når vi har modtaget data fra serveren</summary>
    private void OnNetworkDataReceived(ServerDataPacket networkData)
    {
        //Reset vores ting til en state hvor vi ved de ikke har fået en update
        ResetNetworkedData();

        //Opdater alle vores ting i scenen med nyt data
        HandleNetworkedPlayer(networkData.player);
        HandleNetworkedShips(networkData.enemyShips);
        HandleNetworkedMines(networkData.mines);
        HandleNetworkedCannonBalls(networkData.cannonBalls);

        //Slet alle ting som ikke er blevet opdateret i denne loop
        CleanupUnnetworkedData();
    }

    ///<summary>Resetter vores ting i scenen så vi ved hvad som er opdateret</summary>
    private void ResetNetworkedData()
    {
        for (int j = 0; j < gameManager.GetAllShips().Count; j++)
        {
            gameManager.GetAllShips()[j].ResetNetworkState();
        }

        for (int j = 0; j < gameManager.GetMines().Count; j++)
        {
            gameManager.GetMines()[j].ResetNetworkState();
        }

        for (int j = 0; j < gameManager.GetCannonBalls().Count; j++)
        {
            gameManager.GetCannonBalls()[j].ResetNetworkState();
        }
    }

    ///<summary>Sletter alle uopdaterede ting</summary>
    private void CleanupUnnetworkedData()
    {
        for (int j = 0; j < gameManager.GetAllShips().Count; j++)
        {
            if (gameManager.GetAllShips()[j].GetNetworkState() == false)
            {
                gameManager.OnShipDestroyed(gameManager.GetAllShips()[j]);
            }
        }

        for (int j = 0; j < gameManager.GetMines().Count; j++)
        {
            if (gameManager.GetMines()[j].GetNetworkState() == false)
            {
                gameManager.OnMineDestroyed(gameManager.GetMines()[j]);
            }
        }

        for (int j = 0; j < gameManager.GetCannonBalls().Count; j++)
        {
            if (gameManager.GetCannonBalls()[j].GetNetworkState() == false)
            {
                Destroy(gameManager.GetCannonBalls()[j].gameObject);
            }
        }
    }

    ///<summary>Opdatere vores lokale spiller med nyt data</summary>
    private void HandleNetworkedPlayer(ShipPacketData ship)
    {
        if (ship.ID != -1)
        {
            if (gameManager.GetPlayerShip() != null)
            {
                gameManager.GetPlayerShip().transform.position = ship.position;
                gameManager.GetPlayerShip().transform.rotation = ship.rotation;
                gameManager.GetPlayerShip().SetShipHealth(ship.health, true);
                gameManager.GetPlayerShip().SetShipFuel(ship.fuel, true);
                gameManager.GetUIManager().GetCompass().transform.eulerAngles = new Vector3(gameManager.GetUIManager().GetCompass().transform.eulerAngles.x, gameManager.GetUIManager().GetCompass().transform.eulerAngles.y, gameManager.GetPlayerShip().transform.eulerAngles.y);

                gameManager.GetPlayerShip().UpdateNetworkState();
            }
            else
            {
                gameManager.SpawnPlayerShip(ship.position, ship.rotation, false, ship.ID).UpdateNetworkState();
            }
        }
    }

    ///<summary>Opdatere alle fjendtlige skibe med nyt data</summary>
    private void HandleNetworkedShips(List<ShipPacketData> ships)
    {
        if (gameManager.GetAllShips().Count <= 0)
        {
            for (int i = 0; i < ships.Count; i++)
            {
                gameManager.SpawnEnemyShip(ships[i].position, ships[i].rotation, false, ships[i].ID).UpdateNetworkState();
            }
        }
        else
        {
            for (int i = 0; i < ships.Count; i++)
            {
                bool shipUpdated = false;

                for (int j = 0; j < gameManager.GetAllShips().Count; j++)
                {
                    if (ships[i].ID == gameManager.GetAllShips()[j].GetShipID())
                    {
                        gameManager.GetAllShips()[j].transform.position = ships[i].position;
                        gameManager.GetAllShips()[j].transform.rotation = ships[i].rotation;
                        gameManager.GetAllShips()[j].SetShipHealth(ships[i].health, false);
                        gameManager.GetAllShips()[j].SetShipFuel(ships[i].fuel, false);

                        gameManager.GetAllShips()[j].UpdateNetworkState();

                        shipUpdated = true;

                        break;
                    }
                }

                if (shipUpdated == false)
                {
                    gameManager.SpawnEnemyShip(ships[i].position, ships[i].rotation, false, ships[i].ID).UpdateNetworkState();
                }
            }
        }
    }

    ///<summary>Opdatere alle miner med nyt data</summary>
    private void HandleNetworkedMines(List<MinePacketData> mines)
    {
        if (gameManager.GetMines().Count <= 0)
        {
            for (int i = 0; i < mines.Count; i++)
            {
                gameManager.SpawnMine(mines[i].position, false, mines[i].ID).UpdateNetworkState();
            }
        }
        else
        {
            for (int i = 0; i < mines.Count; i++)
            {
                bool mineUpdated = false;

                for (int j = 0; j < gameManager.GetMines().Count; j++)
                {
                    if (mines[i].ID == gameManager.GetMines()[j].GetMineID())
                    {
                        gameManager.GetMines()[j].UpdateNetworkState();

                        mineUpdated = true;

                        break;
                    }
                }

                if (mineUpdated == false)
                {
                    gameManager.SpawnMine(mines[i].position, false, mines[i].ID).UpdateNetworkState();
                }
            }
        }
    }

    ///<summary>Opdatere alle kanonkugler med nyt data</summary>
    private void HandleNetworkedCannonBalls(List<CannonBallPacketData> cannonBalls)
    {
        if (gameManager.GetCannonBalls().Count <= 0)
        {
            for (int i = 0; i < cannonBalls.Count; i++)
            {
                gameManager.SpawnCannonBalls(gameManager.GetShipByID(cannonBalls[i].ownerID), cannonBalls[i].position, cannonBalls[i].rotation, false, cannonBalls[i].ID).UpdateNetworkState();
            }
        }
        else
        {
            for (int i = 0; i < cannonBalls.Count; i++)
            {
                bool cannonBallUpdated = false;

                for (int j = 0; j < gameManager.GetCannonBalls().Count; j++)
                {
                    if (cannonBalls[i].ID == gameManager.GetCannonBalls()[j].GetCannonBallID())
                    {
                        gameManager.GetCannonBalls()[j].transform.position = cannonBalls[i].position;
                        gameManager.GetCannonBalls()[j].transform.rotation = cannonBalls[i].rotation;

                        gameManager.GetCannonBalls()[j].UpdateNetworkState();

                        cannonBallUpdated = true;

                        break;
                    }
                }

                if (cannonBallUpdated == false)
                {
                    gameManager.SpawnCannonBalls(gameManager.GetShipByID(cannonBalls[i].ownerID), cannonBalls[i].position, cannonBalls[i].rotation, false, cannonBalls[i].ID).UpdateNetworkState();
                }
            }
        }
    }

    ///<summary>Generer en packet med alt nødvendigt information omkring scenen</summary>
    private ServerDataPacket GenerateServerPacket(ServerDataPacket oldPacket)
    {
        ShipPacketData playerData;

        if (gameManager.GetPlayerShip() != null)
        {
            playerData = new ShipPacketData
            {
                ID = gameManager.GetPlayerShip().GetShipID(),
                position = gameManager.GetPlayerShip().transform.position,
                rotation = gameManager.GetPlayerShip().transform.rotation,
                health = gameManager.GetPlayerShip().GetShipHealth(),
                fuel = gameManager.GetPlayerShip().GetShipFuel()
            };
        }
        else
        {
            playerData = new ShipPacketData
            {
                ID = -1,
                position = new Vector3(),
                rotation = new Quaternion(),
                health = 0,
                fuel = 0
            };
        }

        List<ShipPacketData> enemyShipData = new List<ShipPacketData>();

        for (int i = 0; i < gameManager.GetAllShips().Count; i++)
        {
            if(gameManager.GetAllShips()[i] as EnemyShip != null)
            {
                ShipPacketData shipData = new ShipPacketData
                {
                    ID = gameManager.GetAllShips()[i].GetShipID(),
                    position = gameManager.GetAllShips()[i].transform.position,
                    rotation = gameManager.GetAllShips()[i].transform.rotation,
                    health = gameManager.GetAllShips()[i].GetShipHealth(),
                    fuel = gameManager.GetAllShips()[i].GetShipFuel()
                };

                enemyShipData.Add(shipData);
            }
        }

        List<MinePacketData> minesData = new List<MinePacketData>();

        for (int i = 0; i < gameManager.GetMines().Count; i++)
        {
            MinePacketData mineData = new MinePacketData
            {
                ID = gameManager.GetMines()[i].GetMineID(),
                position = gameManager.GetMines()[i].transform.position
            };

            minesData.Add(mineData);
        }

        List<CannonBallPacketData> cannonBallsData = new List<CannonBallPacketData>();

        for (int i = 0; i < gameManager.GetCannonBalls().Count; i++)
        {
            CannonBallPacketData cannonBallData = new CannonBallPacketData
            {
                ID = gameManager.GetCannonBalls()[i].GetCannonBallID(),
                ownerID = gameManager.GetCannonBalls()[i].GetCannonBallOwner().GetShipID(),
                position = gameManager.GetCannonBalls()[i].transform.position,
                rotation = gameManager.GetCannonBalls()[i].transform.rotation
            };

            cannonBallsData.Add(cannonBallData);
        }

        //Indsæt vores data ind i en packet klasse
        ServerDataPacket serverPacket = new ServerDataPacket
        {
            ID = oldPacket.ID + 1,
            fullUpdate = (oldPacket.ID + 1) <= 1,
            gameSession = updateIntervalSession,
            player = playerData,
            enemyShips = enemyShipData,
            mines = minesData,
            cannonBalls = cannonBallsData
        };

        return serverPacket;
    }

    // ----- API funktioner ----- \\

    ///<summary>Initialiser netværks manageren, intet kan køre før vi har kørt den her</summary>
    public void Initialize()
    {
        updateIntervalSession = Utils.RandomString(15);

        updateIntervalActual = 1.0f / updateInterval;

        isInitialized = true;
    }

    ///<summary>Returner true hvis vi er hosten af spillet</summary>
    public bool IsMaster()
    {
        return isGameMaster;
    }

    ///<summary>Sætter os som host af spillet</summary>
    public void SetMaster(bool master)
    {
        isGameMaster = master;
    }

    ///<summary>Returner true hvis vores netværks manager er initialized</summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
}
