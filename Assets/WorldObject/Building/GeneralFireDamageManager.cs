using UnityEngine;
using System; // For Exceprion
using System.Collections;
using System.Collections.Generic;
using System.Linq; //for finding min() and max() in dictionary
using RTS;

namespace RTS{
	
	/// <summary>
	/// A class that manages communication between meshes.
	/// All Updates and future multi-threading will be held here
	/// It is only point where mesh-programmer communication will take place!
	/// </summary>
	public class GeneralFireDamageManager : MonoBehaviour{
		
		private List<MeshManager> meshManagers = new List<MeshManager>();
		List<PairOfAliasesFromDiffrentMeshes> InterMeshPairsList;  //a list of pairs
		FireDamageProperties[] myFireDamageProperties;
		
		private void Start(){
			List<MeshFilter> myMeshFilters = new List<MeshFilter>();
			InterMeshPairsList = new List<PairOfAliasesFromDiffrentMeshes>();
			List<MeshManagerProperties> myMeshManagerProperties = new List<MeshManagerProperties>();
			
			
			
			myFireDamageProperties = GetComponentsInChildren<FireDamageProperties>() as FireDamageProperties[];//
			
			foreach(FireDamageProperties props in myFireDamageProperties){
				try{// I am trying as there may be no meshFilter as a "siblins" of FDP. Somebody made an error and now a exception will be thrown
					MeshFilter CurrentFilter;
					CurrentFilter = props.ReturnMeshFilter();
					myMeshFilters.Add(CurrentFilter);
					myMeshManagerProperties.Add( props.ReturnMeshManagerProperties()  );
				}catch(Exception ex){
					//exception was thrown, so there is no meshFilter found, carry on.
					Debug.Log(ex);
				}
			}	
			
			int i = 0;
			foreach(MeshFilter filter in myMeshFilters){//setting values
					//meshManagers new MeshManager( filter, this, i);
					meshManagers.Add( new MeshManager( filter, this, i, myMeshManagerProperties[i]) );
					i++;
			}
			
			if(meshManagers.Count() > 1){ //when there is only one mesh, there is no place for Inter-Branch connections
				ManageInterMeshConnections(2);
			}
		}
		
		private void Update(){
			if( Input.GetKey(KeyCode.A) ){
				meshManagers[0].Aliases[10].StartFire();
			}
			
			if( Input.GetKey(KeyCode.B) ){
				meshManagers[0].TellAliasesToUpdate();
			}
		}
		
		private void ManageInterMeshConnections(int NumberOfConnections){
			List<PairOfAliasesFromDiffrentMeshes> pairsList = new List<PairOfAliasesFromDiffrentMeshes>();
			
			for(int i=0; i<meshManagers.Count; i++){
				pairsList.AddRange ( FindNumberOfConnectionsOfAnMesh(i, 3) );//ading info from a mesh
			}
			//Okay, now we have to tell these aliases that they are taking part in marvelous operation.
			
			foreach( PairOfAliasesFromDiffrentMeshes pair in pairsList){
				meshManagers[pair.MeshNumber1].Aliases[pair.AliasNumber1].TellThatMakesInterBranchConnections();							
				meshManagers[pair.MeshNumber2].Aliases[pair.AliasNumber2].TellThatMakesInterBranchConnections();
				
				InterMeshPairsList.Add(pair);
			}
			//thats all folks!
		}
		

		
		private List<PairOfAliasesFromDiffrentMeshes> FindNumberOfConnectionsOfAnMesh(int MeshNumber, int NumberOfLinksToFind){
			Dictionary< PairOfAliasesFromDiffrentMeshes, float> TempDictionary = new Dictionary<PairOfAliasesFromDiffrentMeshes, float>();	
			
			for(int i = 0; i<meshManagers.Count(); i++){
				if( i != MeshNumber ){//we want connections from other meshes, eh?
					foreach(Verticle vec1 in meshManagers[MeshNumber].Aliases ){ 
						foreach(Verticle vec2 in meshManagers[i].Aliases){
							PairOfAliasesFromDiffrentMeshes key;
							key.AliasNumber1 = MeshNumber;
							key.MeshNumber1  = vec1.number;
							key.AliasNumber2 = i;
							key.MeshNumber2  = vec2.number;
							
							TempDictionary.Add(key, Vector3.Distance(vec1.positionAbsolute, vec2.positionAbsolute) );	
						}
					}
				}
			}
			
			List< KeyValuePair<PairOfAliasesFromDiffrentMeshes,float> > TempList = TempDictionary.ToList();//we need a list to sort it
			TempList.Sort((firstPair,nextPair) =>/* Magic Sorting!!! using LINQ (maybe) */
			    {
			        return firstPair.Value.CompareTo(nextPair.Value);
			    }
			);
			
			if(NumberOfLinksToFind > TempList.Count()){
				Debug.Log("Sorry, the number of links you want is bigger that thenumber of links we have: NumberOfLinksToFind: "+NumberOfLinksToFind+" Lenght of list with links: "+TempList.Count());
				return TempDictionary.Keys.ToList();
			}else{
				List<PairOfAliasesFromDiffrentMeshes> OutList = new List<PairOfAliasesFromDiffrentMeshes>();
				for(int i=0; i<NumberOfLinksToFind; i++){//making a new list of given lenght!
					OutList.Add(TempList[TempList.Count-i].Key);
				}
				return OutList;
			}
		}
		
		//parameters is Caller data
		public void StartFireOfVerticleInInterMeshConnection(int meshNumber, int AliasNumber){
			foreach(PairOfAliasesFromDiffrentMeshes pair in FindPairsThatHaveAliasOfGivenData(meshNumber, AliasNumber) ){
				meshManagers[pair.MeshNumber1].Aliases[pair.AliasNumber1].StartFire();
				meshManagers[pair.MeshNumber2].Aliases[pair.AliasNumber2].StartFire();
				//calling two Aliases is in fact not requaired as one of them are arleady under fire
			}
		}
		
		private List<PairOfAliasesFromDiffrentMeshes> FindPairsThatHaveAliasOfGivenData(int meshNumber, int AliasNumber){
			List<PairOfAliasesFromDiffrentMeshes> OutList = new List<PairOfAliasesFromDiffrentMeshes>();
			
			foreach(PairOfAliasesFromDiffrentMeshes pair in InterMeshPairsList){
				if(pair.AliasNumber1 == AliasNumber && pair.MeshNumber1 == meshNumber){
					OutList.Add(pair);	
				}
				if(pair.AliasNumber2 == AliasNumber && pair.MeshNumber2 == meshNumber){//there are data about two Aliases in each pair, so this "given" alias may be in two positions
					OutList.Add(pair);	
				}
			}
			return OutList;
		}
		
		public void StartFire( int AliasNumber, int MeshNumber){
			meshManagers[MeshNumber].Aliases[AliasNumber].StartFire();
		}

	}
	
}