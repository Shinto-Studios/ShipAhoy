using UnityEngine;
using UnityEngine.UI;

public class Fuelbar : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // ----- API funktioner ----- \\

    public void SetMaxFuel(int fuel)
    {
        slider.maxValue = fuel;
        slider.value = fuel;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetFuel(int fuel)
    {
        slider.value = fuel;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
