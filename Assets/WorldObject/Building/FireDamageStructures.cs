using UnityEngine;
using System.Collections;
/// <summary>
/// A file with structures used in FireDamage scripts
/// </summary>

namespace RTS{

//a structure serving as "key" for dictionaries looking for connections between aliases from diffrent meshes
public struct PairOfAliasesFromDiffrentMeshes{
	public int MeshNumber1;
	public int MeshNumber2;
	public int AliasNumber1;
	public int AliasNumber2;
}

public struct MeshManagerProperties{
	public float AliasHealthParameter;
	public float InflictDamageParameter;
	public float TryingToBeFiredParameter;
	public bool  MakeMeshThick;
}

	
}
