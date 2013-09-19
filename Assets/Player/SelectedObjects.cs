using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SelectedObjects : MonoBehaviour {
	
	public List<WorldObject> objects;  //List of all currently Selected objects
	public Dictionary<string, int> NumberOfClassObjects ;
	private Player player;
	//public int numberofUnits = 0, numberofBuildings = 0;
	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent<Player>();
			objects = new List<WorldObject>();
		 	NumberOfClassObjects = new Dictionary<string, int>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		
	public void ClearSelection(){//clears the list and sends info to the worldobjects
			while(objects.Count!=0){
				//Debug.Log(objects[0].GetType().BaseType+"X");
				objects[0].SetSelection(false, player.hud.GetPlayingArea());
			}		
			NumberOfClassObjects.Clear();
			
	}
	
	public void MakeSelectionOfOne(WorldObject obj){//one you want to have only one obj selected from that all
		ClearSelection();
		//AddOne(obj);
		obj.SetSelection(true, player.hud.GetPlayingArea());
	}
	
	public void RemoveOne(WorldObject obj){//removes just one of objects from list and send info bout it to him, it shoul be called from worldobject
			for(int i=0; i<objects.Count; i++){
				if(objects[i]==obj){
					//objects[i].SetSelection(false, player.hud.GetPlayingArea());
					objects.RemoveAt(i);
				}
			}	
				 int numberNow = NumberOfClassObjects[obj.GetType().BaseType.ToString()];
				 numberNow = numberNow -1 ;
				if(numberNow == 0){
					NumberOfClassObjects.Remove(obj.GetType().BaseType.ToString());
				}else{
				 NumberOfClassObjects[obj.GetType().BaseType.ToString()] = (numberNow);
				}
				
			Sort();
	}
	
	public void AddOne(WorldObject obj){//add one onject to the list, but not send info to him
			objects.Add(obj);
			int numberNow;
			if(NumberOfClassObjects.ContainsKey(obj.GetType().BaseType.ToString())){
				 numberNow = NumberOfClassObjects[obj.GetType().BaseType.ToString()];

				 numberNow = numberNow + 1;
				 NumberOfClassObjects[obj.GetType().BaseType.ToString()] = (numberNow);
	
			}
			else {numberNow = 0; 
				NumberOfClassObjects.Add (obj.GetType().BaseType.ToString(), (1));
						 	
			};
		Sort();
		  //we incremate the number of cerain objects
					
	}
	

	
	public int Count(){
	int number = 0;
				for(int i=0; i<objects.Count; i++){
						number++;
				}
			    return number;		
	}
	

	
	public void SetHoverState(GameObject obj){ // send SeetHoverState info for all objects
			for(int i=0; i<objects.Count; i++){
				objects[i].SetHoverState(obj);
			}
	}
		
	public void MouseClick(GameObject hitObject, Vector3 hitPoint ,Player player){// make MouseClickMethod to all objects NOT USED NOWLOL
		for(int i=0; i<objects.Count; i++){
				objects[i].MouseClick(hitObject, hitPoint, player);
				
		}	
	}
		
	public bool IfAny(){//check if there is any object in a list
						//Debug.Log(objects.Count);
			int nr = objects.Count;
			if(nr>0){
				return true;
			}
			else{
				return false;
			}
			
	}	
		
	public WorldObject GiveObject(int nr){
		return objects[nr];
	}
	
	public void  Sort(){
		objects.Sort();
	}
	
	public Texture2D GiveObjectsTexture(int nr){//Gives Textures of all objects in form of an array... You give the string name of Type (Unit, Building) and return the tex of objects 
		return objects[nr].buildImage;	
	}
	
	public List<Texture2D> GiveObjectsTexture(string type){//Gives Textures of all objects in form of an array... You give the string name of Type (Unit, Building) and return the tex of objects 
		List<Texture2D> tex = new List<Texture2D>();
		for(int i=0; i<objects.Count; i++){
				if(objects[i].GetType().BaseType.ToString()==type){
					tex.Add(objects[i].buildImage);
				}
		}
		return tex;	
	}
	
	public List<string> GiveObjectsName(string type){//Gives Textures of all objects in form of an array... You give the string name of Type (Unit, Building) and return the name of objects
		List<string> str = new List<string>();
		for(int i=0; i<objects.Count; i++){
				if(objects[i].GetType().BaseType.ToString()==type){
					str.Add(objects[i].objectName);
				}
		}
		return str;	
	}	
	
	public List<float> GiveObjectsHealthPercentage(string type){//Gives Health percentage of all objects in form of an array... You give the string name of Type (Unit, Building) and return the health of objects
		List <float> per = new List<float>();
		for(int i=0; i<objects.Count; i++){
				if(objects[i].GetType().BaseType.ToString()==type){
					per.Add(objects[i].healthPercentage);
				}
		}
		return per;	
	}
	
	public void SelectAllBuildings(){
		ClearSelection(); Debug.Log("JUS");
		Building[] AllBuildings = FindObjectsOfType(typeof (Building)) as Building[];
		foreach (Building building in AllBuildings){
			if(building.player == player){
				building.SetSelectionMultiple(true, player.hud.GetPlayingArea());
			}
		}
	}
	
	public void SelectAllUnits(){
		ClearSelection(); Debug.Log("JUkS");
		Unit[] AllUnits = FindObjectsOfType(typeof (Unit)) as Unit[];
		foreach (Unit unit in AllUnits){
			if(unit.player == player){
				unit.SetSelectionMultiple(true, player.hud.GetPlayingArea());
			}
		}
	}
	
		/*public void UpdateUnitsNumber(){
			if(objects.Count>0){
				 numberofUnits = 0;
				 numberofBuildings = 0;
				for(int i=0;i<objects.Count; i++){
					//Debug.Log(objects[i].GetType().BaseType.ToString()+"Unit and now SSS Building++>");
					if(objects[i].GetType().BaseType.ToString()=="Unit"){
						numberofUnits++;
						
					}
					if(objects[i].GetType().BaseType.ToString()=="Building"){
						numberofBuildings++;

					}
				}
			}
		}*/
}

