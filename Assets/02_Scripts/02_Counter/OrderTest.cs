using System.Collections.Generic;
using UnityEngine;

public class OrderTest : MonoBehaviour
{
    public OrderGenerator generator;
    public IngredientDatabase ingredientDB;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Order order = generator.GenerateOrder();

            string noodleName = ingredientDB.GetIngredientByID(order.noodleID).name;

            List<string> toppingNames = new List<string>();
            foreach (int id in order.toppingIDs)
            {
                toppingNames.Add(
                    ingredientDB.GetIngredientByID(id).name
                );
            }

            string message = order.GenerateOrderMessage(noodleName, toppingNames);

            Debug.Log(message);
        }
        
    }
}