using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTS;

public class MeshManager {//for each meshfilter there should be an distinct meshManager
	
	public Mesh mesh;
	private MeshFilter filter;
	public Vector3 ParentPosition;
	public List<Verticle> Aliases;
	public int[] VerticleToAliasArray;//the index is vert number, in its there is the number of its alias, its size is the number of veerticles
	public List<Vector3> Triangles;
	public List<int> WallsWaitingToBeMade;
	
	float MinimumHeight;
	float MaximumHeight;
	
	private VerticleState meshState;
	private float time = 0;
	public int OrgTriangleCount = 0;
	
	public bool IsMeshThick = false;
	
	public float DebDestroyTime = 0f;
	public float DebGeneralDestroyTime = 0f;
	public int DebDestroyCount = 0;
	public int DebGeneralNumber = 0;
	
	public bool MeshWasChanged = false;
	private int[] TempMeshTrinaglesArray;
	private Vector3[] TempMeshVerticlesArray;
	private int NumberOfGroundAliases = 0;
	private int[] AliasToBranchArray;
	
	public float AliasHealthParameter = 30f;
	public float InflictDamageParameter = 0.25f;
	public float TryingToBeFiredParameter = 1f;
	private List <Vector2> PlannedTrianglesToBeMadeBetweenAliasAndTwin;
	
	public MeshManager (ref MeshFilter meshFilter){//constructor
		Initialise();
		
		ParentPosition = meshFilter.transform.position;
		filter = meshFilter;
		mesh = meshFilter.mesh;
		
		ManageVerticles(mesh.vertices);
		ManageTriangles(mesh.triangles);
		CalcualateMinimumHeight();
		CalculateNormals();
		
		FindAliasesBranches();
		CheckUnClosedMeshes(); 
		MakeMeshThick();	
		ProducePlannedTrianglesBetweenAliasAndTwin();
	}
	

	
	private void Initialise () {
		Aliases = new List<Verticle>();
		Triangles = new List<Vector3>();
		meshState = VerticleState.Standard;
		WallsWaitingToBeMade = new List<int>();
		PlannedTrianglesToBeMadeBetweenAliasAndTwin = new List<Vector2>();
	}
	
	private void ManageVerticles(Vector3[] verticles){//fnction find where many verticles are in the same positions and in that way produces its aliases
		Dictionary<Vector3, List<int>> TempAliases = new Dictionary<Vector3, List<int>>(); //list of ints are numbers of verticles in the specific position (the Vector3)
		
		int i = 0;
		foreach (Vector3 verticle in verticles){
			if(TempAliases.ContainsKey(verticle)){//we've arleady added a verticle of this position, so the alias will have multiple verticles
				TempAliases[verticle].Add(i);
			}else{//we have to add a number of our verticle to our list
				TempAliases[verticle] = new List<int>();
				TempAliases[verticle].Add(i);
			}
			i++;	
		}
		VerticleToAliasArray = new int[i];
		
		i = 0;
		foreach(var pair in TempAliases){//a Key-Value Pair
			Aliases.Add (new Verticle(this, i, pair.Value, pair.Key));
			ManageVerticleToAliasArray(i,pair.Value);
			i++;
		}
	}
	
	private void ManageVerticlesAddNewUpdateOld(Vector3[] verticles){
		Dictionary<Vector3, List<int>> TempAliases = new Dictionary<Vector3, List<int>>(); //list of ints are numbers of verticles in the specific position (the Vector3)

		int i = 0; 
		foreach (Vector3 verticle in verticles){
			if(TempAliases.ContainsKey(verticle)){//we've arleady added a verticle of this position, so the alias will have multiple verticles
				TempAliases[verticle].Add(i); 
			}else{//we have to add a number of our verticle to our list
				TempAliases[verticle] = new List<int>();
				TempAliases[verticle].Add(i);
			}
			i++;	
		}
		VerticleToAliasArray = new int[i];
		
		i = 0;
		int OrgAliasesCount = Aliases.Count; 
		foreach(var pair in TempAliases){//a Key-Value Pair
			if(i>OrgAliasesCount-1){//we add a new verticle
				Aliases.Add (new Verticle(this, i, pair.Value, pair.Key));
			}else{
				Aliases[i].Modify(i, pair.Value, pair.Key);	
			}
			ManageVerticleToAliasArray(i,pair.Value);
			i++;		
		}
	
	}
	
	private void ManageVerticleToAliasArray(int i, List<int> list){//we are trying to fill the Verticle to Alias Array
		foreach(int k in list){
			VerticleToAliasArray[k] = i;	
		}
	}
	
	private void ManageTriangles(int[] triangles){//info about triangles is stored in an array. blblblba

		for(int i=0; i<triangles.Length; i+=3){
			Triangles.Add(new Vector3(triangles[i], triangles[i+1], triangles[i+2]));//ok, we have a nice list of triangles, what now?
		}

		
		int j = 0;
		foreach(Vector3 vect in Triangles){//we pass info to verticle bout the triangles it particip in
			Aliases[VerticleToAliasArray[(int)vect.x]].AddTriangle(j,ChangeVerticleToAlias(vect));
			Aliases[VerticleToAliasArray[(int)vect.y]].AddTriangle(j,ChangeVerticleToAlias(vect));
			Aliases[VerticleToAliasArray[(int)vect.z]].AddTriangle(j,ChangeVerticleToAlias(vect));
			j++;
		}
	}
	
	public Vector3 ChangeVerticleToAlias(Vector3 vect){
		Vector3 x = new Vector3(VerticleToAliasArray[(int)vect.x], VerticleToAliasArray[(int)vect.y], VerticleToAliasArray[(int)vect.z]);
		return x;
	}
	
	public void CalcualateMinimumHeight(){//Counting absolute minimum and maximum height of verticle in the mesh
		Vector3[] verticles = mesh.vertices;	
		foreach(Verticle vert in Aliases){
			if(vert.positionAbsolute.y < MinimumHeight){ MinimumHeight = vert.positionAbsolute.y;}
			if(vert.positionAbsolute.y > MaximumHeight){ MaximumHeight = vert.positionAbsolute.y;}
		}
	}
	
	public void CalculateNormals(){//Counting absolute minimum and maximum height of verticle in the mesh
		foreach(Verticle vec in Aliases){
			vec.CalculateNormal();	
		}
	}
	
	public void TranslateFromMeshToManager(){
		Triangles = new List<Vector3>(); ;
		ManageVerticlesAddNewUpdateOld(mesh.vertices);
		ManageTriangles(mesh.triangles);
		CalcualateMinimumHeight();
		if(IsMeshThick==true){
			for(int i=Aliases.Count/2; i<Aliases.Count; i++){
				Aliases[i].IsATwin = true;
			}
		}
	}
	
	public void Destroy (float percent){ 
		float DestroyHeight	= MaximumHeight -(MaximumHeight - MinimumHeight)*percent;// ParentPosition.y + 0.66f;
		foreach(Verticle vert in Aliases){
			vert.DestroyByHeight(DestroyHeight);
		}
		

	}
	
	public void FFire(int verticleNumber){
		Aliases[verticleNumber].StartFire();
	}
	
	public void RemoveTriangle(int k){

		Triangles[k] = Vector3.zero;
	}
	
	public void UpdateTrianglesList(){//gets the in-manager triangle list and then translates it to mesh.triangles format, and then sets it to be the mesh.triangles. If mesh.traingles is longer than Traingles*3, writes from 0 to traingles*3, rest of mesh.traingles is left unchanged
		//Debug.Log("UPL");
		int LenghtOfTrianglesArray = Triangles.Count*3;
		int[] tempTrianglesArray = new int[LenghtOfTrianglesArray];
		int i = 0;
		foreach(Vector3 vec in Triangles){
			tempTrianglesArray[i] 	= (int)vec.x;
			tempTrianglesArray[i+1] = (int)vec.y;
			tempTrianglesArray[i+2] = (int)vec.z;
			i+=3;
		}
		int[] tempTrianglesArrayNumber2 = new int[mesh.triangles.Length];
		mesh.triangles.CopyTo(tempTrianglesArrayNumber2,0);
		tempTrianglesArray.CopyTo(tempTrianglesArrayNumber2, 0);

		mesh.triangles = tempTrianglesArrayNumber2;
		//mesh.triangles = tempTrianglesArray;
	}
	
	public void UpdateVerticlesList(){
		
		Vector3[] temp = new Vector3[VerticleToAliasArray.Length];
		
		for(int i=0; i< VerticleToAliasArray.Length; i++){
			int k = VerticleToAliasArray[i];
			temp[i] = Aliases[k].positionRelative;
		}
		mesh.vertices = temp;		
		
	}
	
	public void DestroyByNumber(int numb){
		Aliases[numb].DestroyByNumber(numb);	
			MakeWallsFromList();
			UpdateTrianglesList();
			TranslateFromMeshToManager();
	}
	
	public void FireAliasesBySphere(Vector3 pos){
		/*float SphereRadius = 0.5f;
		foreach(Verticle ver in Aliases){
			if(Vector3.Distance(ver.positionRelative, pos)<SphereRadius){
				ver.StartFire();	
			}
		}*/
	}
	
	public void TellAliasesToUpdate(){	
		
		DebDestroyCount = 0;
		DebDestroyTime = 0;
		
		
		if(time>1){
			for(int k=0; k<Aliases.Count;k++){
				Aliases[k].Update();
			}
			time = 0;
		}else{
			time += Time.deltaTime;	
		}
		

		if(MeshWasChanged){
			
			MakeWallsFromList();
			UpdateTrianglesList();
			TranslateFromMeshToManager();
			MeshWasChanged = false;

		}
		
		if(DebDestroyCount>0){
			DebGeneralNumber++;
			DebGeneralDestroyTime += DebDestroyTime;
			
		}
	}
	
	
	public void MakeMeshThick(){
		OrgTriangleCount = (mesh.triangles.Length) /3;
		AddTwinsToVerticleArray();
		AddTwinsToTriangleAlias();
		AddTwinsToUvArray();
		
		TranslateFromMeshToManager();
		for(int i=Aliases.Count/2; i<Aliases.Count; i++){
			Aliases[i].IsATwin = true;
		}
		IsMeshThick = true;
	}
	
	private void AddTwinsToVerticleArray(){
		Vector3[] OldVerticleList = mesh.vertices;
		Vector3[] NewVerticleList = new Vector3[OldVerticleList.Length];
				
		float Wall_Thickness = 0.01f;
		Vector3 TargetPosition;
		Vector3 NewPosition ;

		for(int i=0; i<OldVerticleList.Length; i++){
			
			TargetPosition = mesh.vertices[i] - Aliases[VerticleToAliasArray[i]].normal;
			NewPosition = Vector3.MoveTowards(mesh.vertices[i], TargetPosition, Wall_Thickness);
			NewVerticleList[i] = NewPosition;
		}
		
		Vector3[] final = new Vector3[OldVerticleList.Length + NewVerticleList.Length];
		OldVerticleList.CopyTo(final, 0);
		NewVerticleList.CopyTo(final, OldVerticleList.Length);
		
		mesh.vertices = final;
	}
	
	private void AddTwinsToTriangleAlias(){
		int[] OldTriangleList = mesh.triangles;
		int[] NewTriangleList = new int [OldTriangleList.Length];
		int offset = (mesh.vertices.Length/2); 
		for(int i=0; i<OldTriangleList.Length; i+=3){
			NewTriangleList[i] = OldTriangleList[ i + 2 ] + offset;
			NewTriangleList[i+1] = OldTriangleList[ i + 1 ] + offset;
			NewTriangleList[i+2] = OldTriangleList[ i ] + offset;
		}
		
		int[] final = new int[OldTriangleList.Length + NewTriangleList.Length];
		OldTriangleList.CopyTo(final, 0);
		NewTriangleList.CopyTo(final, OldTriangleList.Length);
		
		mesh.triangles = final;
	}
	
	private void AddTwinsToUvArray(){
		Vector2[] OldUvList = mesh.uv;//what is important here is that there arleady is automaticly-generated uv. We have to ovverride them.
		Vector2[] TwinUv = new Vector2[OldUvList.Length/2];
		//int offset = OldUvList.Length/2;
		for(int i=0; i<OldUvList.Length/2; i++){
			TwinUv[i] = OldUvList[i];	
		}
		TwinUv.CopyTo(OldUvList,OldUvList.Length/2);
		mesh.uv = OldUvList;
	}
	
	public int AddTriangleAndVerticles(int a, int b, int c){//gets 3 numbers of Aliases. Produces a triangle between them, and new verticles
		Vector3[] OldVerticleList = TempMeshVerticlesArray;
		Vector3[] NewVerticleList = new Vector3 [TempMeshVerticlesArray.Length+3];
		Vector3[] LastThreeVerts = new Vector3[3]{Aliases[a].positionRelative,Aliases[b].positionRelative,Aliases[c].positionRelative  };
		OldVerticleList.CopyTo(NewVerticleList,0);
		LastThreeVerts.CopyTo(NewVerticleList, TempMeshVerticlesArray.Length);
		TempMeshVerticlesArray = NewVerticleList;
		
		
		int[] OldTriangleList = TempMeshTrinaglesArray;
		int[] NewTriangleList = new int [TempMeshTrinaglesArray.Length+3];
		int[] NewTriangle = new int[3]{TempMeshVerticlesArray.Length-1, TempMeshVerticlesArray.Length-2, TempMeshVerticlesArray.Length-3};
		OldTriangleList.CopyTo(NewTriangleList, 0);
		NewTriangle.CopyTo(NewTriangleList, OldTriangleList.Length);
		TempMeshTrinaglesArray = NewTriangleList;
		return (TempMeshTrinaglesArray.Length/3)-1;  //the number of added triangle
	}
	
	public void AddWallToWallsList(int a, int b, int c){
		WallsWaitingToBeMade.Add(a);
		WallsWaitingToBeMade.Add(b);
		WallsWaitingToBeMade.Add(c);
	}
	
	private void MakeWallsFromList(){//this check if verticles of teoretical Alias are still Ok. If so, sends info to make trinagle from them.
		TempMeshVerticlesArray = new Vector3[mesh.vertices.Length];
		mesh.vertices.CopyTo(TempMeshVerticlesArray,0);
		
		TempMeshTrinaglesArray = new int[mesh.triangles.Length];
		mesh.triangles.CopyTo(TempMeshTrinaglesArray,0);
		
		float time = Time.realtimeSinceStartup;
		for(int i=0; i<WallsWaitingToBeMade.Count; i+=3){
			bool ItIsOk = true;
			
			if(WallsWaitingToBeMade[i]<Aliases.Count/2 && (Aliases[WallsWaitingToBeMade[i]].state != VerticleState.Destroyed)){
			//well, do nothin ,its ok
			}else{
				if(WallsWaitingToBeMade[i]>=Aliases.Count/2){//it is a Twin!, so dont have state
					if(	Aliases[WallsWaitingToBeMade[i]-Aliases.Count/2].state != VerticleState.Destroyed ){//check the Alias
					//well, do nothin	
					}else{ItIsOk = false;}
				}else{ItIsOk = false;}
			}
			
			if(WallsWaitingToBeMade[i+1]<Aliases.Count/2 && (Aliases[WallsWaitingToBeMade[i+1]].state != VerticleState.Destroyed)){
			//well, do nothin
			}else{
				if(WallsWaitingToBeMade[i+1]>=Aliases.Count/2){//it is a Twin!, so dont have state
					if(	Aliases[WallsWaitingToBeMade[i+1]-Aliases.Count/2].state != VerticleState.Destroyed ){//check the Alias
					//well, do nothin	
					}else{ItIsOk = false;}
				}else{ItIsOk = false;}
			}
			
			if(WallsWaitingToBeMade[i+2]<Aliases.Count/2 && (Aliases[WallsWaitingToBeMade[i+2]].state != VerticleState.Destroyed)){
			//well, do nothin
			}else{
				if(WallsWaitingToBeMade[i+2]>=Aliases.Count/2){//it is a Twin!, so dont have state
					if(	Aliases[WallsWaitingToBeMade[i+2]-Aliases.Count/2].state != VerticleState.Destroyed ){//check the Alias
					//well, do nothin	
					}else{ItIsOk = false;}
				}else{ItIsOk = false;}
			}
			
			if(ItIsOk == true){
				AddTriangleAndVerticles(	WallsWaitingToBeMade[i], WallsWaitingToBeMade[i+1], WallsWaitingToBeMade[i+2]);	
			}
		}
			
		
		WallsWaitingToBeMade.Clear();
			mesh.vertices = TempMeshVerticlesArray;
			mesh.triangles = TempMeshTrinaglesArray;
		DebDestroyTime += (Time.realtimeSinceStartup - time);
	}
	
	private void SetGroundAliases(float percent){
		float CuttingPosition = MaximumHeight - (MaximumHeight-MinimumHeight)*percent;
		foreach(Verticle v in Aliases){
			if(v.positionAbsolute.y<CuttingPosition){
				v.state = VerticleState.Ground;
				NumberOfGroundAliases++;
			}
		}

	}
//////////////////////////////////////////////////////////////////////////////////////////////	

	
	private void FindAliasesBranches(){

		AliasToBranchArray = new int[Aliases.Count];
		for(int i=0; i<AliasToBranchArray.Length; i++){//initialise the array with -1 meaning that value of this index is not yet set
			AliasToBranchArray[i] = -1;	
		}
		int CurrentBranchNumber = 0;
		
		foreach(Verticle vec in Aliases){//Give AliasToBranchArray values 
			if(AliasToBranchArray[vec.number]==-1){//this alias is not yet set to any branch
				AliasToBranchArray[vec.number] = CurrentBranchNumber;
				GetConnectedAliases(vec.number, CurrentBranchNumber);
				CurrentBranchNumber++;
			}
		}
		if(CurrentBranchNumber > 0){//only one branch, so we dont need to look for connections
			for(int i=0; i<CurrentBranchNumber; i++){
				List<Vector2> FinalConnections = FindNumberOfInterBranchConnections(i, 2);	
				foreach(Vector2 vec in FinalConnections){
					Aliases[(int)vec.x].AddLinkFromOtherBranch((int)vec.y);	
					Aliases[(int)vec.y].AddLinkFromOtherBranch((int)vec.x);
					
				}
			}
		}
		
	}
	
	private void GetConnectedAliases(int number, int CurrentBranchNumber){	
		List<int> LinkedAliases = Aliases[number].LinkedAliases;
		foreach(int i in LinkedAliases){
			if(AliasToBranchArray[i]==-1){
				AliasToBranchArray[i] = CurrentBranchNumber;
				GetConnectedAliases(i, CurrentBranchNumber);
			}
		}
	}
	
	private List<Vector2> FindNumberOfInterBranchConnections(int BranchNumber, int HowManyConnections){//it searches for This branch Connections (Aliases close to themselves). One Alias can make only One connection!
		List<Vector2> ConnectionsList = new List<Vector2>(); //Vector2.x is the Alais in our branch Vector2.y is the closest Alias in other branch
		foreach(Verticle vec in Aliases){//here we are trying to get list of closest inter-branch connections of every Alias in breanch
			if(AliasToBranchArray[vec.number] == BranchNumber){
				ConnectionsList.Add ( new Vector2(vec.number, FindCloseAliasNotInTheSameBranch(vec.number, BranchNumber)) );
			}
		}
		
		List<Vector2> FinalConnections = FindNumberOfClosestConnections(ConnectionsList, 2);
		
		return FinalConnections;
	}
	
	private List<Vector2> FindNumberOfClosestConnections(List<Vector2> InList, int NumberOfClosestConnections){
		Dictionary<Vector2, float> DistanceDictionary = new Dictionary<Vector2, float>(); //Key is the pair, value is the distance between	
		foreach(Vector2 pair in InList){//lets fill the distionary with distances between each pair
			//Debug.Log("pair x is"+pair.x+" and y is "+pair.y);
			float Distance = Vector3.Distance(Aliases[(int)pair.x].positionRelative, Aliases[(int)pair.y].positionRelative);
			DistanceDictionary.Add (pair, Distance);
		}
		
		Dictionary<Vector2, float> ClosestConnections = FindANumberOfSmallestElementsInDictionary(DistanceDictionary, NumberOfClosestConnections);
		
		List<Vector2> OutList = new List<Vector2>();
		foreach(KeyValuePair<Vector2, float> pair in ClosestConnections){
			OutList.Add (pair.Key);	
		}
		
		return OutList;
	}
	
	private int FindCloseAliasNotInTheSameBranch(int number, int BranchNumber){//finds and returns the Alias closest to Alias of given number, not being part of branch
		int CurrentClosestAlias = -1;
		float Distance = 1000f; //Just very big number
		
		foreach(Verticle vec in Aliases){
			if(vec.number!=number){//verticle is not the being checked verticle
				if(AliasToBranchArray[vec.number]!=BranchNumber){//not from the same branch
					if(Vector3.Distance(Aliases[number].positionRelative, vec.positionRelative) <	Distance){
						CurrentClosestAlias = vec.number;
						Distance = Vector3.Distance(Aliases[number].positionRelative, vec.positionRelative);
					}
				}
			}
		}
		
		return CurrentClosestAlias;
		
	}
	
	private Dictionary<Vector2, float> FindANumberOfSmallestElementsInDictionary(Dictionary<Vector2, float> InDictionary, int NumberOFSmallest){//returns a given number of  pairs with smallest values; 
		if(NumberOFSmallest>InDictionary.Count){ Debug.Log("Well, the count of given list is smaller than the amount of smallest things you want. That is wrong"); };	
		Dictionary<Vector2, float> OutDictionary = new Dictionary<Vector2, float>();
		for(int i=0; i<NumberOFSmallest; i++){//lets add tome default data
			OutDictionary.Add(new Vector2((float)i,0f), 10000f);	
		}
		
		foreach(KeyValuePair<Vector2, float> pair in InDictionary){
			foreach(KeyValuePair<Vector2, float> pair2 in OutDictionary){
				if(pair2.Value> pair.Value){
					OutDictionary.Remove(pair2.Key);
					OutDictionary.Add (pair.Key, pair.Value);
					break;
				}
			}
		}
		
		return OutDictionary;
		
	}
	////////////////////////////////////	LookingForBranchconnectionsEndshere
	
	///////////////////////////////////     Checking unended mesh-parts starts here
	
	private void CheckUnClosedMeshes(){//tutorial, part X
		//Okay, what do we need? We need a list of edges that belond to only one triangle!
		Dictionary <float, Vector3> EdgesList = new Dictionary<float, Vector3>(); //This is intresting. When we will add new edges, we will duplicate some data
			//So how to find duplicates? I decided to use Dictionary. We will be given a "edge data" consisting of two Alias numbers. They will form Vector3.x and Vector3.y
			//Using them we are going to form a  (float) key that will be diffrent for every two diffent numbers (Alias numbers). Vector3.z will be the number of times this edge will appear
			//than by checking the Vector3.z data we will find duplicates.
		
		foreach(Vector3 tri in Triangles){ // as we know, in each traingle there are three edges. Moreover data in Traingles is stored as verticles, not triangles data
			TryToAddEdgeToEdgesDictionary( VerticleToAliasArray[ (int) tri.x ], VerticleToAliasArray[ (int) tri.y ], EdgesList);
			TryToAddEdgeToEdgesDictionary( VerticleToAliasArray[ (int) tri.y ], VerticleToAliasArray[ (int) tri.z ], EdgesList);
			TryToAddEdgeToEdgesDictionary( VerticleToAliasArray[ (int) tri.x ], VerticleToAliasArray[ (int) tri.z ], EdgesList);
		}
	
		
		
		//Okay, lets find the edges which are in only one triangle
		foreach( KeyValuePair<float, Vector3> pair in EdgesList){
		
			if(pair.Value.z == 0){
				AddToPlannedTrianglesBetweenAliasAndTwin( (int)	pair.Value.x, (int) pair.Value.y);
			}
		}
		
	}
	
	private float ProduceSeed(int x, int y){// unique seed of two ints
		int a, b;
		if(x>y){ a = x; b = y;}
		else { a = y; b = x ;}//these manevuers are made to produce seed identical no mater in what order the parameters were send. When we give alias numbers 12 and 15 or 15 and 12 the same effect will happen
		
		return (a * Aliases.Count + b  );// Not sure how random it is, hope that enough random	
	}
	
	private KeyValuePair<float, Vector3> MakeOneEdgeToDictionary(int x, int y){//does what name says, used in CheckUnClosedMeshes method
		return new KeyValuePair<float, Vector3>( ProduceSeed(x,y) , new Vector3(x, y, 0)); //As number of times the edge was found (Vector3.z) zero is now given. Has to be modified in caller method!
	}
	
	private void TryToAddEdgeToEdgesDictionary(int x, int y, Dictionary<float, Vector3> EdgesList){ //given numbers are alias numbers
		if( x == y ){//Sometimes happen, but should not. Anyway, calculating anything then has no point
			return;
		}
		
		KeyValuePair<float, Vector3> Temp;
		Temp = MakeOneEdgeToDictionary(x, y) ;
		if(EdgesList.ContainsKey(Temp.Key)){ //check is this pair was arleady added. If so, increment Vector3.z Else, add this edge
			Vector3 Current = EdgesList[Temp.Key]; 
			Current.z = Current.z +1;
			EdgesList[Temp.Key] = Current;
		}else{
			EdgesList.Add(Temp.Key, Temp.Value); 
		}
	}
	
	private void AddToPlannedTrianglesBetweenAliasAndTwin(int x, int y){//we want to store data about which new edges has to produce walls.
		//this is becouse calculating this edges is done BEFORE making Mesh Thick. As in making walls we use twins, it has to be done later,
		//after meshes twins are placed. This wall making is done in ProducePlannedTrianglesBetweenWalls
		PlannedTrianglesToBeMadeBetweenAliasAndTwin.Add( new Vector2( x, y ) );
	}
	
	
	private void ProducePlannedTrianglesBetweenAliasAndTwin(){
		if(IsMeshThick == false){
			Debug.Log("Mesh is not thick. Becouse of that I cannot make triangles as there is not twins! ");
			return;	
		}else{
			foreach(Vector2 vec in PlannedTrianglesToBeMadeBetweenAliasAndTwin){//Procedures similar to ones used in Verticle class, during wall-making after a destruction of Alias
				AddWallToWallsList( (int) vec.x						, (int) vec.y					, (int) vec.x + Aliases.Count/2);
				AddWallToWallsList( (int) vec.y						, (int) vec.x + Aliases.Count/2	, (int) vec.y + Aliases.Count/2);
				AddWallToWallsList( (int) vec.x + Aliases.Count/2	, (int) vec.y					, (int) vec.x );
				AddWallToWallsList( (int) vec.y + Aliases.Count/2	, (int) vec.x + Aliases.Count/2	, (int) vec.y );
				
			}
			
			MakeWallsFromList();
			UpdateTrianglesList();
			TranslateFromMeshToManager();
			
		}
		
	}
	
	
}
	
