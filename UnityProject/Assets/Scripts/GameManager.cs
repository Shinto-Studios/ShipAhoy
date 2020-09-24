using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    ///<summary>Prefabs til at spawne ting ind på mappet</summary>
    [SerializeField] private GameObject minePrefab = null;
    [SerializeField] private GameObject enemyShipPrefab = null;
    [SerializeField] private GameObject playerShipPrefab = null;
    [SerializeField] private GameObject cannonBallPrefab = null;

    ///<summary>Main kameraret i scenen, den bliver automatisk assigned til det første kamera i scenen hvis det har tagget "MainCamera"</summary>
    [SerializeField] private Camera mainCamera = null;

    ///<summary>Variabler til de andre essentielle klasser vi bruger</summary>
    [SerializeField] private UIManager UiManager = null;
    [SerializeField] private AudioManager audioManager = null;
    [SerializeField] private FXManager fxManager = null;
    [SerializeField] private NetworkManager networkManager = null;

    ///<summary>Bliver brugt til at spawne minerne og de fjendtlige skibe</summary>
    [SerializeField] private int spawnEnemies = 10;
    [SerializeField] private int spawnMines = 5;

    [SerializeField] private float spawnRadiusEnemies = 50.0f;
    [SerializeField] private float spawnRadiusMines = 50.0f;

    ///<summary>Vores scene data omkring hvad der er på mappet</summary>
    private readonly List<Mine> mines = new List<Mine>();
    private List<IShip> shipsAll = new List<IShip>();
    private List<CannonBall> cannonBalls = new List<CannonBall>();
    private PlayerShip shipPlayer = null;

    ///<summary>Bliver brugt til at tjekke om spillet er igang</summary>
    private bool isPlaying = false;

    // ----- Engine funktioner ----- \\

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if(isPlaying == true)
        {
            if(GetNetworkManager().IsInitialized())
            {
                if (GetNetworkManager().IsMaster())
                {
                    //Kør logikken for alle skibe i scenen hvis vi er hosten
                    for (int i = 0; i < shipsAll.Count; i++)
                    {
                        shipsAll[i].OnUpdate();
                    }

                    if (Input.GetKeyDown(KeyCode.K))
                    {
                        SpawnEnemyShip(new Vector3(Random.Range(-spawnRadiusEnemies, spawnRadiusEnemies), 0, Random.Range(-spawnRadiusEnemies, spawnRadiusEnemies)), Quaternion.identity, true);
                    }
                }

                //Kør alt netværks logik
                GetNetworkManager().OnUpdate();
            }

            ///<summary>Få main kameraret til at følge vores spiller</summary>
            if (shipPlayer != null)
            {
                mainCamera.transform.localPosition = new Vector3(shipPlayer.transform.position.x, 60.0f, shipPlayer.transform.position.z);
                mainCamera.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
            }
        }
    }

    private void FixedUpdate()
    {
        ///<summary>Lav physics updates for alle skibene hvis vi er hosten</summary>
        if (GetNetworkManager().IsMaster())
        {
            for (int i = 0; i < shipsAll.Count; i++)
            {
                shipsAll[i].OnFixedUpdate();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ///<summary>Tjek om det er spille scenen og instantiate alle tingene på mappet hvis det er, samt skaf nogle ui ting</summary>
        if (scene.name == "Main")
        {
            mainCamera = Camera.main;

            GetUIManager().Initialize();

            GetNetworkManager().Initialize();

            if (GetNetworkManager().IsMaster() == true)
            {
                SpawnPlayerShip(Vector3.zero, Quaternion.identity, true);

                for (int i = 0; i < spawnEnemies; i++)
                {
                    SpawnEnemyShip(new Vector3(Random.Range(-spawnRadiusEnemies, spawnRadiusEnemies), 0, Random.Range(-spawnRadiusEnemies, spawnRadiusEnemies)), Quaternion.identity, true);
                }

                for (int i = 0; i < spawnMines; i++)
                {
                    SpawnMine(new Vector3(Random.Range(-spawnRadiusMines, spawnRadiusMines), 0, Random.Range(-spawnRadiusMines, spawnRadiusMines)), true);
                }
            }

            isPlaying = true;
        }
        else
        {
            isPlaying = false;
        }
    }

    // ----- Custom funktioner ----- \\

    ///<summary>Indlæser spil scenen</summary>
    public void PlayGame(bool isMaster)
    {
        GetNetworkManager().SetMaster(isMaster);

        SceneManager.LoadScene("Main");
    }

    ///<summary>Afslutter spillet helt</summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    ///<summary>Bliver kaldt når et skib bliver ødelagt</summary>
    public void OnShipDestroyed(IShip ship)
    {
        //Tjek om det er vores spiller som er død
        if (ship as PlayerShip != null)
        {
            shipPlayer = null;
        }

        //Fjern skibet fra vores liste
        shipsAll.Remove(ship);

        //Ik spørg hvorfor, men det giver en større eksplosion, vi var for dovne her til at lave en seperat større eksplosions prefab xd
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);
        GetFXManager().ExplosionAtPos(ship.transform.position);

        //Slet det helt til sidst
        Destroy(ship.gameObject);
    }

    ///<summary>Bliver kaldt når en mine bliver ødelagt</summary>
    public void OnMineDestroyed(Mine mine)
    {
        //Igen, vi var for dovne her til at lave et seperat prefab bare til en lidt større eksplosion
        GetFXManager().ExplosionAtPos(mine.transform.position);
        GetFXManager().ExplosionAtPos(mine.transform.position);

        mines.Remove(mine);

        Destroy(mine.gameObject);
    }

    ///<summary>Bliver kaldt når en mine bliver ødelagt</summary>
    public void OnCannonBallDestroyed(CannonBall ball)
    {
        cannonBalls.Remove(ball);

        Destroy(ball.gameObject);
    }

    ///<summary>Spawner et spiller skib og assigner det til playerShip til senere brug</summary>
    public PlayerShip SpawnPlayerShip(Vector3 position, Quaternion rotation, bool isMaster, int ID = -1)
    {
        //For at skaffe et ordenligt id skal vi altid skaffe det sidst skib i listen og give den et id større end den
        if (ID == -1)
        {
            ID = shipsAll.Count > 0 ? shipsAll.Last().GetShipID() + 1 : 1;
        }

        //Har vi allerede en eksisterende spiller fjerner ham først før vi instantiere vores nye
        if (shipPlayer != null)
        {
            shipsAll.Remove(shipPlayer);

            Destroy(shipPlayer.gameObject);
        }

        //Instantier ham og kald de nødvendige funktioner
        PlayerShip shipPlayerInstantiate = Instantiate(playerShipPrefab, position, rotation).GetComponent<PlayerShip>();
        shipPlayerInstantiate.ShipCreatedHandler(this, isMaster, ID);

        shipPlayer = shipPlayerInstantiate;
        shipsAll.Add(shipPlayerInstantiate);

        return shipPlayerInstantiate;
    }

    ///<summary>Spawner et fjende skib og assigner det til shipEnemies listen til senere brug</summary>
    public EnemyShip SpawnEnemyShip(Vector3 position, Quaternion rotation, bool isMaster, int ID = -1)
    {
        //For at skaffe et ordenligt id skal vi altid skaffe det sidst skib i listen og give den et id større end den
        if (ID == -1)
        {
            ID = shipsAll.Count > 0 ? shipsAll.Last().GetShipID() + 1 : 1;
        }

        //Instantier fjenden og kald de nødvendige funktioner
        EnemyShip shipEnemyInstantiate = Instantiate(enemyShipPrefab, position, rotation).GetComponent<EnemyShip>();
        shipEnemyInstantiate.ShipCreatedHandler(this, isMaster, ID);

        //shipsEnemies.Insert(ID, shipEnemyInstantiate);
        shipsAll.Add(shipEnemyInstantiate);

        return shipEnemyInstantiate;
    }

    ///<summary>Spawner en mine og assigner det til mines listen til senere brug</summary>
    public Mine SpawnMine(Vector3 position, bool isMaster, int ID = -1)
    {
        //For at skaffe et ordenligt id skal vi altid skaffe det sidst skib i listen og give den et id større end den
        if (ID == -1)
        {
            ID = mines.Count > 0 ? mines.Last().GetMineID() + 1 : 1;
        }

        Mine mineInstantiate = Instantiate(minePrefab, position, Quaternion.identity).GetComponent<Mine>();
        mineInstantiate.MineCreatedHandler(this, isMaster, ID);

        mines.Add(mineInstantiate);

        return mineInstantiate;
    }

    ///<summary>Spawner en mine og assigner det til mines listen til senere brug</summary>
    public CannonBall SpawnCannonBalls(IShip ship, Vector3 position, Quaternion rotation, bool isMaster, int ID = -1)
    {
        //For at skaffe et ordenligt id skal vi altid skaffe det sidst skib i listen og give den et id større end den
        if (ID == -1)
        {
            ID = cannonBalls.Count > 0 ? cannonBalls.Last().GetCannonBallID() + 1 : 1;
        }

        GetAudioManager().PlayCannonSoundAtPos(position);
        GetFXManager().FireBlastAtPos(position);

        CannonBall cannonBall = Instantiate(cannonBallPrefab, position, rotation).GetComponent<CannonBall>();
        cannonBall.CannonBallCreatedHandler(ship, isMaster, ID);

        cannonBalls.Add(cannonBall);

        return cannonBall;
    }

    // ----- API funktioner ----- \\

    ///<summary>Finder skibet som har et bestemt ID</summary>
    public IShip GetShipByID(int ID)
    {
        for (int j = 0; j < GetAllShips().Count; j++)
        {
            if (ID == GetAllShips()[j].GetShipID())
            {
                return GetAllShips()[j];
            }
        }

        return null;
    }

    ///<summary>Skaffer alle kanon skud som er affyret</summary>
    public List<CannonBall> GetCannonBalls()
    {
        return cannonBalls;
    }

    ///<summary>Skaffer alle miner på banen</summary>
    public List<Mine> GetMines()
    {
        return mines;
    }

    ///<summary>Skaffer alle fjendtlige skibe</summary>
    public List<IShip> GetAllShips()
    {
        return shipsAll;
    }

    ///<summary>Skaffer spiller skibet</summary>
    public PlayerShip GetPlayerShip()
    {
        return shipPlayer;
    }

    // ----- API Manager funktioner ----- \\

    ///<summary>Skaffer UI Manageren</summary>
    public UIManager GetUIManager()
    {
        return UiManager;
    }

    ///<summary>Skaffer Audio Manageren</summary>
    public AudioManager GetAudioManager()
    {
        return audioManager;
    }

    ///<summary>Skaffer FX Manageren</summary>
    public FXManager GetFXManager()
    {
        return fxManager;
    }

    ///<summary>Skaffer Netværks Manageren</summary>
    public NetworkManager GetNetworkManager()
    {
        return networkManager;
    }
}
