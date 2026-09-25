using UnityEngine;

// The ItemClick script lets us inspect items by clicking on them
public class ItemClick : MonoBehaviour
{
    void Update()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any collider in the scene
            if (Physics.Raycast(ray, out hit))
            {
                // Try to get an Item component from whatever we hit.
                // Note we ask for 'Item', not 'HealthPotion' - so this works for every item type.
                Item clickedItem = hit.transform.GetComponent<Item>();

                if (clickedItem != null)
                {
                    clickedItem.DisplayInfo();
                }
            }
        }
    }
}