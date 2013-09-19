using UnityEngine;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject {
	
	public float maxBuildProgress;
	public Texture2D rallyPointImage, sellImage;
	
	protected Vector3 spawnPoint, rallyPoint;
	protected Queue<string> buildQueue;
	
	private float currentBuildProgress = 0.0f;
	private bool needsBuilding = false;
	
	public List<MeshManager> meshManagers;
	
	private float[][] MinimumHeights ;
	private float MinimumHeight = 1000;
	private float MaximumHeight = -1000;
	private Vector3[,] OrgVerticles ;
	public int[] VerticleState;	//State of Verticle 0 - ok 1 - burning 2 -burned, dont move
	public int[] triangles;
	public int NumOfMeshfilters;
	
	/*** Game Engine methods, all can be overridden by subclass ***/
	
	protected override void Awake() {
		base.Awake();
		buildQueue = new Queue<string>();
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		rallyPoint = spawnPoint;
		
	}
	
	protected override void Start () {
		base.Start();
		//CalcualateMinimumHeight();
		InitialiseMeshMenagers();

	}
	
	protected override void Update () {
		base.Update();
		ProcessBuildQueue();
		
		for(int i=0; i<meshManagers.Count;i++){
				meshManagers[i].TellAliasesToUpdate();
		}
	}
	
	protected override void OnGUI() {
		base.OnGUI();
		if(needsBuilding) DrawBuildProgress();
	}
	
	/*** Internal worker methods that can be accessed by subclass ***/
	
	protected void CreateUnit(string unitName) {
		buildQueue.Enqueue(unitName);
	}
	
	protected void ProcessBuildQueue() {
		if(buildQueue.Count > 0) {
			currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
			if(currentBuildProgress > maxBuildProgress) {
				if(player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
				currentBuildProgress = 0.0f;
			}
		}
	}
	
	/*** Public methods ***/
	
	public string[] getBuildQueueValues() {
		string[] values = new string[buildQueue.Count];
		int pos=0;
		foreach(string unit in buildQueue) values[pos++] = unit;
		return values;
	}
	
	public float getBuildPercentage() {
		return currentBuildProgress / maxBuildProgress;
	}
	
	public override void SetSelection(bool selected, Rect playingArea) {
		base.SetSelection(selected, playingArea);
		if(player) {
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if(selected) {
				if(flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition) {
					flag.transform.localPosition = rallyPoint;
					flag.transform.forward = transform.forward;
					flag.Enable();
				}
			} else {
				if(flag && player.human) flag.Disable();
			}
		}
	}
	
	public bool hasSpawnPoint() {
		return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}
	
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			if(hoverObject.name == "Ground") {
				if(player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
			}
		}
	}
	
	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.MouseClick(hitObject, hitPoint, controller);
		//only handle iput if owned by a human player and currently selected
		if(player && player.human && currentlySelected) {
			if(hitObject.name == "Ground") {
				if((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition) {
					SetRallyPoint(hitPoint);
				}
			}
		}
	}
	
	public void SetRallyPoint(Vector3 position) {
		rallyPoint = position;
		if(player && player.human && currentlySelected) {
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if(flag) flag.transform.localPosition = rallyPoint;
		}
	}
	
	public void Sell() {
		if(player) player.AddResource(ResourceType.Money, sellValue);
		if(currentlySelected) SetSelection(false, playingArea);
		Destroy(this.gameObject);
	}
	
	public void StartConstruction() {
		CalculateBounds();
		needsBuilding = true;
		hitPoints = 0;
	}
	
	public bool UnderConstruction() {
		return needsBuilding;
	}
	
	public void Construct(int amount) {
		hitPoints += amount;
		if(hitPoints >= maxHitPoints) {
			hitPoints = maxHitPoints;
			needsBuilding = false;
			RestoreMaterials();
		}
	}
	
	/*** Private Methods ***/
	
	private void DrawBuildProgress() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the main draw area
		GUI.BeginGroup(playingArea);
		CalculateCurrentHealth(0.5f, 0.99f);
		DrawHealthBar(selectBox, "Building ...");
	}
	
	private void InitialiseMeshMenagers(){
		meshManagers = new List<MeshManager>();
		MeshFilter[] myMeshFilters = GetComponentsInChildren<MeshFilter>();
		for(int i=0; i<myMeshFilters.Length;i++){
			if(myMeshFilters[i].mesh.triangles.Length>0){
				meshManagers.Add(new MeshManager(ref myMeshFilters[i]));
			}
		}
	}
	
	public void FireDamage(){//changing height of every verticle!, going down
		foreach(MeshManager manager in meshManagers){
			//manager.DestroyByNumber(88);

			manager.FFire(10);

		}
		/*int j=0;
		MeshFilter[] myMeshFilters = GetComponentsInChildren<MeshFilter>();
		//meshManager = new MeshManager(myMeshFilters[0]);
		
		foreach(MeshFilter meshFilter in myMeshFilters){
			Vector3[] verticles = meshFilter.mesh.vertices;
			Vector3 ParentPosition = meshFilter.transform.position;
			Vector3 LastPosition = new Vector3(0,0,0);
			//Debug.Log(meshFilter.mesh.triangles.Length);	

			int i = 0;
			
			while(i< verticles.Length){
				if(verticles[i].y+0.3f<OrgVerticles[j,i].y){
					verticles[i] = LastPosition;
				}else{
						verticles[i].y -= Mathf.PerlinNoise(verticles[i].y+ParentPosition.y+verticles[i].x+ParentPosition.x, verticles[i].z+ParentPosition.z)*0.1f;
						//Well, U can ast why hese perlinNoises and other stuff. Well it is becouse the In every triangle of mesh there are 3 separate verticles. That mean that in Tetrahedron there are 12 verticles instead of 4.
						// i 'd use Random on everyone, they would simply split up. I want when in the same place there are 3 verticles to every verticle move in the same (random) way.
					LastPosition = verticles[i];
				}
				i++;
			}
			meshFilter.mesh.vertices = verticles;
			j++;
		}*/
		
	}
	
	/*public void CalcualateMinimumHeight(){//Wyliczanie absolutnego najniższego i najwyzszego miejsca w pferabie, oraz miejsc gdzie najnizej moze byc dany verticle!
		MeshFilter[] myMeshFilters = GetComponentsInChildren<MeshFilter>();
		
		int i = 0;
		int j = 0; //number meshfiltru!
		int FullNumberOfVerticles = 0;
		
		foreach(MeshFilter meshFilter in myMeshFilters){//we count how many meshes and verticles in meshes there are
			Vector3[] verticles = meshFilter.mesh.vertices;	
			while(i< verticles.Length){
				i++;
			}
			j++;
		}
		NumOfMeshfilters = j;
		
		OrgVerticles = new Vector3[j,i];
		i = 0;
		j = 0;
		
		
		foreach(MeshFilter meshFilter in myMeshFilters){	//we calculate the org positions of verticles, 
			i = 0;
			triangles = meshFilter.mesh.triangles;
			Vector3[] verticles = meshFilter.mesh.vertices;	
			while(i< verticles.Length){
				OrgVerticles[j,i] = verticles[i];
				i++;
				FullNumberOfVerticles++;
			}
			j++;
		}
		

		VerticleState = new int[FullNumberOfVerticles];
		//set every part of verticlestate to 0, cous nothin in being on fire!
		foreach(int k in VerticleState){
			VerticleState[k] = 0;	
		}
		
		
		foreach(MeshFilter meshFilter in myMeshFilters){
			i = 0;
			Vector3[] verticles = meshFilter.mesh.vertices;	
			Vector3 ParentPosition = meshFilter.transform.position;
			while(i< verticles.Length){
				if(verticles[i].y+ParentPosition.y < MinimumHeight){ MinimumHeight = verticles[i].y+ParentPosition.y;}
				if(verticles[i].y+ParentPosition.y > MaximumHeight){ MaximumHeight = verticles[i].y+ParentPosition.y;}
				i++;
			}
	 	
		}
		
		
		
		
	}*/
	
	
	
	
}
