using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS{
	public class Verticle {
		
		private MeshManager OwnerManager;
		public VerticleState state;
		public int number;//my own number of this alias
		public List<int> Aliases;	
		public List<int> Triangles;

		public List<int> LinkedAliases;//Aliases of verts with which this vert is making triangles
		public List<int> LinksToOtherBranches;
		public Vector3 positionRelative;
		public Vector3 positionAbsolute;
		public Vector3 normal;
		public bool IsATwin = false;
		
		private float health;
		private bool TryingToBeFired = false;
		
		
		public void Update(){
			if(state == VerticleState.Destroyed){
				return;
			}
			
			if(state == VerticleState.Ground){
				return;
			}
			
			if(health<=0){
				Destroy();
				state = VerticleState.Destroyed;
				return;
			}
			
			if(state == VerticleState.Burning){
				InflictDamage();
				TryToFireLinkedAliases();
				TryToFireLinkedAliasesFromOtherBranches();
			}
			
			if(TryingToBeFired&&state != VerticleState.Burning){
				state = VerticleState.Burning;	
			}
				
		}
		
		public Verticle(MeshManager Owner ,int Xnumber, List<int> XAliases, Vector3 XpositionRelative){
			Initialise();
			OwnerManager = Owner;
			number = Xnumber;
			Aliases = XAliases;
			positionRelative = XpositionRelative;
			//normal = OwnerManager.mesh.normals[Aliases[0]];
			SetPositionAbsolute();
		}
		
		private void Initialise(){
			Triangles = new List<int>();
			LinkedAliases = new List<int>();
			Aliases = new List<int>();
			state = VerticleState.Standard;
			positionAbsolute = Vector3.zero;
			health = 1.0f;
			LinksToOtherBranches = new List<int>();
		}
		
		public void Modify(int Xnumber, List<int> XAliases, Vector3 XpositionRelative){  //this updates some elements, but saves ones like health
			Triangles = new List<int>();		//////Initialisation
			LinkedAliases = new List<int>();	//
			
			number = Xnumber;
			Aliases = XAliases;
			positionRelative = XpositionRelative;	
			normal = OwnerManager.mesh.normals[Aliases[0]];
			SetPositionAbsolute();
		}
		
		public void AddOffsetToAliases(int offset){
			for(int i=0; i<Aliases.Count; i++){
				Aliases[i] += offset;	
			}
		}
		
		public void SetPositionAbsolute(){
			positionAbsolute = 	OwnerManager.ParentPosition;
			positionAbsolute = positionAbsolute + positionRelative;
			
		}
		
		public void SetPositionRelative(Vector3 Pos){
			Pos = positionRelative;	
		}
		
		public void CalculateNormal(){//calculates the average normal
			Vector3 TempNormal = Vector3.zero;
			float HowMany = 0;
			foreach( int k in Aliases){
				TempNormal += OwnerManager.mesh.normals[k];
			}
			normal = TempNormal/(Aliases.Count);	
		}
		
		public void AddTriangle(int i, Vector3 vert){//vert is a vector with aliases of vector participating in triangle
			if(Triangles.Contains(i)){//if we arleady have it, there is no point in adding trangle number
				return;
			}
			Triangles.Add (i);
			
			CheckIfAddLinkedAlias((int)vert.x);
			CheckIfAddLinkedAlias((int)vert.y);
			CheckIfAddLinkedAlias((int)vert.z);

		}
		
		
		private void CheckIfAddLinkedAlias(int x){ 
			if((!Aliases.Contains(x)) && (!LinkedAliases.Contains(x)) ){
				LinkedAliases.Add(x);
			}
		}
		
		public void AddLinkFromOtherBranch(int i){ 
			if(!LinksToOtherBranches.Contains(i)){
				LinksToOtherBranches.Add(i);	
			}
			//DebEnlightenThisAlias();
			//DebEnlightenAAlias(i, Color.magenta);
		}
		
		public void DestroyByHeight(float height){
			if(height<positionAbsolute.y){
				Destroy();
			}
		}
		
		public void DestroyByNumber(int numb){
			if(numb==number){
				Destroy();	
			}
		}
		
		public void DestroyBeingTwin(){
			if(IsATwin == true){
				Destroy();	
			}
		}
		
		private void Destroy(){
			//if(number==100){Debug.Log("Linked Alias is "+LinkedAliases[1]);}
			OwnerManager.MeshWasChanged = true;
			
			float time = 0f;
			time = Time.realtimeSinceStartup;
			
			for(int i = 0; i<LinkedAliases.Count; i++){
				OwnerManager.Aliases[  LinkedAliases[i]  ].RemoveLink(number);
				foreach(int l in Triangles){
					OwnerManager.Aliases[  LinkedAliases[i]  ].RemoveLink(number);
				}
				RemoveLink(  LinkedAliases[i]  );
			}
			RemoveAllLinksToOtherBranches();
			
			if((OwnerManager.IsMeshThick==true)&&IsATwin==false){
				OwnerManager.Aliases[number+(OwnerManager.Aliases.Count/2)].Destroy();	
				ProduceTrianglesBetweenWalls();
			}
			DestroyTriangles();
			//OwnerManager.UpdateTrianglesList();
			
			time =  Time.realtimeSinceStartup - time;
			//Debug.Log("Time made is "+time);
			OwnerManager.DebDestroyTime += time;
			OwnerManager.DebDestroyCount++;
		}
				
		public void RemoveLink(int k){
			int index = LinkedAliases.IndexOf(k);
			LinkedAliases.Remove(index);
		}
		
		private void RemoveAllLinksToOtherBranches(){//Deletes the connections to LTOB in this and other Aliases
			foreach(int i in LinksToOtherBranches){
				OwnerManager.Aliases[i].LinksToOtherBranches.Remove(number);//delete the link in other alias to this  Alias
			}
			LinksToOtherBranches.Clear();
		}
		

		
		private void DestroyTriangles(){


			for(int i = 0 ; i<Triangles.Count; i++){
				int k = Triangles[i];
				OwnerManager.RemoveTriangle(k);
			}

			
		}
		
		
		public void RemoveTriangle(int number){
			if(Triangles.Contains(number)){
				Triangles.Remove(number);	
			}
		}
		
		public void StartFire(){ 
			if(!TryingToBeFired){
				TryingToBeFired = true;
			}
		}
		
		public void InflictDamage(){
			health -= 0.5f;
		}
		
		public void TryToFireLinkedAliases(){
			//OwnerManager.FireAliasesBySphere(positionRelative);
			foreach(int k in LinkedAliases){
					OwnerManager.Aliases[k].StartFire();
			}
		}
		
		private void TryToFireLinkedAliasesFromOtherBranches(){
			foreach(int i in LinksToOtherBranches){
				OwnerManager.Aliases[i].StartFire();	
			}
		}
		
		private void ProduceTrianglesBetweenWalls(){//fills the void between Alias and its twin
		
			int[] TempListOfAliasNumbers= new int[2];  //Stores the numbers of other (not this) aliases that form a triangle
			int DestroyedAliasPosition = 0;
			foreach(int k in Triangles){	
				if(k < OwnerManager.OrgTriangleCount){//This triangle is not a wall triangle
					Vector3 VertNumbers = OwnerManager.Triangles[k];
					if(VertNumbers != Vector3.zero){
						int i = 0;
						for(int j=0; j<3; j++){
							
							float f = VertNumbers[j];
							if(OwnerManager.VerticleToAliasArray[ (int) f]!= number){
								int z = OwnerManager.VerticleToAliasArray[ (int) f];
								TempListOfAliasNumbers[i] = z;
								i++;
							}else{ DestroyedAliasPosition = i; }
						}
						if( (OwnerManager.Aliases[TempListOfAliasNumbers[0]].state != VerticleState.Destroyed) && (OwnerManager.Aliases[TempListOfAliasNumbers[1]].state != VerticleState.Destroyed) ){
							AddTriangleInOrder(DestroyedAliasPosition, TempListOfAliasNumbers);
						}
					}
				}
			}
		}
		
		private void AddTriangleInOrder(int DestroyedAliasPosition, int[] TempListOfAliasNumbers){
			
			switch (DestroyedAliasPosition){
				case 0:
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2,TempListOfAliasNumbers[1], TempListOfAliasNumbers[0])   ;
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[1] + OwnerManager.Aliases.Count/2,TempListOfAliasNumbers[1],  TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2)  ;
					break;
				case 1: 
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[0]                               ,TempListOfAliasNumbers[1],  TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2)  ;
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2,TempListOfAliasNumbers[1],  TempListOfAliasNumbers[1] + OwnerManager.Aliases.Count/2)  ;
					break;
				case 2:
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2,TempListOfAliasNumbers[1],  TempListOfAliasNumbers[0] )  ;
					OwnerManager.AddWallToWallsList(TempListOfAliasNumbers[1] + OwnerManager.Aliases.Count/2,TempListOfAliasNumbers[1],  TempListOfAliasNumbers[0] + OwnerManager.Aliases.Count/2)  ;
					break;	

			}

		}
		
		private void DebEnlightenAVert(int i, Color col){
			Vector3 pos = (OwnerManager.mesh.vertices[i] + OwnerManager.ParentPosition);
			Debug.DrawLine (Vector3.zero, pos, col);
		}
		
		private void DebEnlightenAAlias(int i, Color col){
			DebEnlightenAPoint(OwnerManager.Aliases[i].positionAbsolute, col); 
			Debug.Break();
		}
		
		private void DebEnlightenAPoint(Vector3 pos, Color col){
			Debug.DrawLine (Vector3.zero, pos, col);
			
		}
		 
		private void DebEnlightenThisAlias(){
			Vector3 pos = positionAbsolute;
			Debug.DrawLine (Vector3.zero, pos, Color.red);
			Debug.Break();
		}
		
		private void DebEnlightenTriangles(){
			foreach(int i in Triangles){
 
					DebEnlightenAVert((int) OwnerManager.Triangles[i][0],  Color.blue);	
					DebEnlightenAVert((int) OwnerManager.Triangles[i][1],  Color.blue);
					DebEnlightenAVert((int) OwnerManager.Triangles[i][2],  Color.blue);
			}
			
		}
		
		private void DebEnlightenASpecificTriangle(int i){
 
					DebEnlightenAVert((int) OwnerManager.Triangles[Triangles[i]][0],  Color.blue);	
					DebEnlightenAVert((int) OwnerManager.Triangles[Triangles[i]][1],  Color.blue);
					DebEnlightenAVert((int) OwnerManager.Triangles[Triangles[i]][2],  Color.blue);
					Debug.Break();
		}
		
		private void DebEnlightenLinkedAliases(){
			foreach(int i in LinkedAliases){
				DebEnlightenAVert(OwnerManager.Aliases[i].Aliases[0],Color.green);
			}
			Debug.Break();
		}
		
		private void DebEnlightenATriangle(int i, Color col){
			Vector3 vec = OwnerManager.Triangles[Triangles[i]];
			DebEnlightenAVert((int)vec.x, col);
			DebEnlightenAVert((int)vec.y, col);
			DebEnlightenAVert((int)vec.z, col);
		}
		
		public void DebWriteAliases(){
			string s = "Hi, mine number ist "+number;
			foreach(int i in Aliases){
				s +=(" and "+i);	
			}
			Debug.Log(s);
		}
		

}
	
}