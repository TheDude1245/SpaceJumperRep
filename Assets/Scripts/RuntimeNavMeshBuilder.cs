// RuntimeNavMeshBuilder.cs
using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]
public class RuntimeNavMeshBuilder : MonoBehaviour
{
	private NavMeshSurface surface;

	void Awake()
	{
		surface = GetComponent<NavMeshSurface>();
		surface.BuildNavMesh(); // builds automatically when the scene starts
	}
}
