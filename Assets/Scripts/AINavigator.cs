using UnityEngine;

public class AINavigator : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    public GameObject destination;
    
    private void Update()
    {
        // Convertir transform.position en myPosition
        Vector2Int myPosition = (Vector2Int)Vector3Int.FloorToInt(transform.position);
        
        // Récupère direction
        Vector2Int myDirection = FlowFieldManager.Instance.GetDirectionAtPosition(myPosition,  transform.position);
        
        // Convertir myDirection en direction
        Vector3 direction = new Vector3
        {
            x = myDirection.x,
            y = myDirection.y
        };
        
        // Movement
        transform.Translate(direction * (Time.deltaTime * moveSpeed));
    }
}