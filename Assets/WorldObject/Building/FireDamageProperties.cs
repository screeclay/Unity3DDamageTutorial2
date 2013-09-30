using UnityEngine;
using System.Collections;
using System; // For exception
using RTS;

/// <summary>
/// A class which job is to hold properties set by map maker. 
/// It should be added to every mesh which should take part in fire damage.
/// </summary>
public class FireDamageProperties : MonoBehaviour{
	public float AliasHealthParameter = 30f;
	public float InflictDamageParameter = 0.25f ;
	public float TryingToBeFiredParameter = 1f;
	public bool  MakeMeshThick = true;
	MeshFilter[] filter;
	
	private void Awake(){
		filter = GetComponentsInChildren<MeshFilter>();
	
	}
	
	
	/// <summary>
	/// Returns a (sibling) mesh filter to the FireDamageProperties if found, throws System.NullReferenceException if not
	/// </summary>
	/// <returns>
	/// The mesh filter if it was found, a System.NullReferenceException; if not
	/// </returns>
	public MeshFilter ReturnMeshFilter(){
		
		if(filter != null){//a MeshFilter was found!
			return filter[0];
		}else{
			throw new Exception("Filter not found");
		}
	}
	
	public MeshManagerProperties ReturnMeshManagerProperties(){
		MeshManagerProperties properties;
		
		properties.AliasHealthParameter = AliasHealthParameter;
		properties.InflictDamageParameter = InflictDamageParameter;
		properties.MakeMeshThick = MakeMeshThick;
		properties.TryingToBeFiredParameter = TryingToBeFiredParameter;
		
		return properties;
	}
	
}
