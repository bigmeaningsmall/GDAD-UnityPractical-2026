
// Another plain C# class - no MonoBehaviour, no Unity overhead 
// This holds the shared identity that EVERY item has. - Its the template for an item

[System.Serializable]
public class ItemData
{
    public string itemName;
    public string description;

    // Default constructor - a generic, unnamed item
    public ItemData()
    {
        itemName = "Generic Item";
        description = "A generic item that does things.";
    }

    // Parameterised constructor - each item type stamps its own identity
    public ItemData(string newItemName, string newDescription)
    {
        itemName = newItemName;
        description = newDescription;
    }
}