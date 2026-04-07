// data about one animal — hunger, happiness, production
[System.Serializable]
public class AnimalData
{
    public enum AnimalType { Cow, Sheep, Chicken }

    public string animalName;
    public AnimalType animalType;

    [UnityEngine.Range(0, 100)]
    public int hunger = 50;       // 0 = starving, 100 = full

    [UnityEngine.Range(0, 100)]
    public int happiness = 50;    // 0 = miserable, 100 = happy

    public bool productionReady;  // true = can collect milk/wool/egg
}
