using UnityEngine;
using System.Collections;
using RTS;


public class SelectRectangle : MonoBehaviour {
	
	private Player player;
	public Rect Rectangle;
	public bool draw;
	private bool EnoughBig;
	public GUISkin mySkin;
	public GameObject SelectRectangleMesh;
	public int i;
	private Camera cam;
    private Plane[] planes;
	GameObject[] VisibleObjects;
	public Vector3 res, pos;
	public float mousex, mousey;


	
	// Use this for initialization
	void Start () {	
		player = transform.root.GetComponent<Player>();
		draw = false;
		EnoughBig = false;
	}
	
	// Update is called once per frame
	void Update () {
	mousex = Input.mousePosition.x;
	mousey = Input.mousePosition.y;		
	}
	
	public void SetStartCoordinates(Vector3 pos){ 
		draw = true;
		Rectangle.x =  pos.x;
		Rectangle.y = Screen.height - pos.y ;

	}
	
	public void SetEndCoordinates(Vector3 pos){						
			Rectangle.width =  pos.x - Rectangle.x;
			Rectangle.height = -(pos.y - (Screen.height -Rectangle.y)); //Why? I have no idea, but its working!			
	}
	
	public void MakeSelection(){

			if(draw&&CheckIfBigEnough()){
				player.selectedObjects.ClearSelection();
				GetAllObjects(Rectangle);
			}	
			
			draw = false;
			
	}
	
	public bool CheckIfBigEnough(){
		//Debug.Log(Rectangle.width+"-=-"+Rectangle.height+draw);
		if(Mathf.Abs(Rectangle.width)+Mathf.Abs(Rectangle.height)>30){
			return true;
		}
		else{
			return false;
		}
	}
	
	void OnGUI(){
		DebugDrawObjectPoints();
		if(draw){
			GUI.skin = mySkin;
			GUI.Label(Rectangle, "");
		}
	}
	
	void GetAllObjects(Rect XSelectBox){	
		////////////////This part is responsible for changing the begin  coordinates for Selectbox. You see, width and height can be positive, as well as negatve, couse initial coordinates can be in evr corner of rect. After this function the initial coords are left top i think, anyway height and width are positive!
		Rect SelectBox = new Rect(0,0,1,1);
		if(XSelectBox.width>0){
			SelectBox.x = XSelectBox.x;
		}
		else{ SelectBox.x = XSelectBox.x+XSelectBox.width;}
		if(XSelectBox.height>0){
			SelectBox.y = XSelectBox.y;
		}
		else{ SelectBox.y = XSelectBox.y+XSelectBox.height;}
		SelectBox.width = Mathf.Abs(XSelectBox.width);
		SelectBox.height = Mathf.Abs(XSelectBox.height);
		/////////////////////////////////////////
		
		cam = Camera.main;
		Building[] allBuildings = Building.FindObjectsOfType(typeof(Building)) as Building[];
		foreach(Building thisBuilding in allBuildings){
			  pos = thisBuilding.transform.position;
			  res = cam.WorldToViewportPoint( pos);
			 res.y = (res.y-1)*(-1); // We have to flip is, make symmetry couse apparently sth has bottom lest 00 cords, and sth other has top left (0,0) coords
			  res.x = res.x*Screen.width;
			  res.y = res.y*Screen.height;
			
			if(res.x>0&&res.y>0&&res.z>0){
				if(SelectBox.Contains(res)){
						//Debug.Log("thru lev2");	
						//player.selectedObjects.AddOne(thisBuilding);
						thisBuilding.SetSelectionMultiple(true, player.hud.GetPlayingArea());
					
				}
			}
		}
		
		Unit[] allUnits = Unit.FindObjectsOfType(typeof(Unit)) as Unit[];
		foreach(Unit thisUnit in allUnits){
			  pos = thisUnit.transform.position;
			  res = cam.WorldToViewportPoint( pos);
			res.y = (res.y-1)*(-1); // We have to flip is, make symmetry couse apparently sth has bottom lest 00 cords, and sth other has top left (0,0) coords
			  res.x = res.x*Screen.width;
			  res.y = res.y*Screen.height;
			 
			if(res.x>0&&res.y>0&&res.z>0){
				if(SelectBox.Contains(res)){
						//Debug.Log("LOL");
						//player.selectedObjects.AddOne(thisUnit);
						thisUnit.SetSelectionMultiple(true, player.hud.GetPlayingArea());
					
				}
			}
		}
		//player.selectedObjects.UpdateUnitsNumber();
	}
	
	public void DebugDrawObjectPoints(){ //draws rectangles in places on screen where worldobjects are!
		cam = Camera.main;
		WorldObject[] allobjects = Building.FindObjectsOfType(typeof(WorldObject)) as WorldObject[];	
		foreach (WorldObject thisobj in allobjects){
			  pos = thisobj.transform.position;
			  res = cam.WorldToViewportPoint( pos);
			  res.y = (res.y-1)*(-1); // We have to flip is, make symmetry couse apparently sth has bottom lest 00 cords, and sth other has top left (0,0) coords
			  //Debug.Log(res);
			  res.x = res.x*Screen.width;
			  res.y = res.y*Screen.height;
				
			  GUI.skin = mySkin;
			  GUI.Label (new Rect(res.x,res.y,5,5),"W");
		}
	}
}

