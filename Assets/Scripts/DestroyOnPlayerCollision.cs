using UnityEngine;

public class DestroyOnPlayerCollision : MonoBehaviour
{
	[Header("Tag Settings")]
	public string playerTag = "Player"; // make sure your player has this tag!

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag(playerTag))
		{
			Debug.Log($"{gameObject.name} was destroyed by player collision!");
			Destroy(gameObject);
		}
	}
}
