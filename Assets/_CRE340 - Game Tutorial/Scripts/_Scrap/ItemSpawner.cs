using UnityEngine;
using System.Collections.Generic;

// The ItemSpawner places Health and Mana Potions in the scene
public class ItemSpawner : MonoBehaviour
{
    // References to the potion prefabs, to be set in the Unity Inspector
    public GameObject healthPotionPrefab;  // Drag the HealthPotion prefab here
    public GameObject manaPotionPrefab;    // Drag the ManaPotion prefab here

    public int numberOfItemsEachSide = 3;  // Number of items each side of the origin
    public float spacing = 2.0f;           // Distance between each item along the X-axis

    // A list that holds EVERY item we spawn - as the base type 'Item'.
    // A HealthPotion and a ManaPotion can sit side by side in here because both ARE Items.
    private List<Item> allItems = new List<Item>();

    void Start()
    {
        SpawnRow(healthPotionPrefab, 0f);     // health potions along Z = 0
        SpawnRow(manaPotionPrefab, -4.0f);    // mana potions along Z = -4 to separate them visually

        // Just to test the item inheritance - loop through every item, whatever type it is, and call the same method
        DisplayAllItems();
    }

    // One spawn method handles BOTH prefabs, because both are Items
    void SpawnRow(GameObject itemPrefab, float zPosition)
    {
        for (int i = -numberOfItemsEachSide; i <= numberOfItemsEachSide; i++)
        {
            // Work out the position along the X-axis
            Vector3 position = new Vector3(i * spacing, 0.5f, zPosition);

            // Instantiate takes 3 parameters (GameObject, position, rotation)
            GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);

            // Grab the Item component - this works for ANY item type
            Item itemComponent = newItem.GetComponent<Item>();

            if (itemComponent != null)
            {
                allItems.Add(itemComponent); // store it in our list of Items - we don't need to save a list but I want to show what a list is.
            }
            else
            {
                Debug.LogWarning("The spawned object does not have an Item component!");
            }
        }
    }

    // Loop through the list and call the same method on every item - this is just to test calling the item function to show it doesn't care what the item is as long as its base class is item with the DisplayInfo function
    void DisplayAllItems()
    {
        Debug.Log("--- Listing all spawned items ---");

        foreach (Item item in allItems)
        {
            item.DisplayInfo();
        }
    }
}