using UnityEngine;
using System.Collections.Generic;
using RTS;

public class HUD : MonoBehaviour {
	
	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, ObjectSkin,ObjectCatSkin, mouseCursorSkin;
	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor, rallyPointCursor, minusCursor, plusCursor;
	public Texture2D[] attackCursors, harvestCursors, moveCursors;
	public Texture2D[] resources, resourceHealthBars;
	public Texture2D buttonHover, buttonClick, smallButtonHover, smallButtonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D healthy, damaged, critical;
	
	private Player player;
	private CursorState activeCursorState, previousCursorState;
	private int currentFrame = 0, buildAreaHeight = 0;
	private Dictionary<ResourceType,int> resourceValues, resourceLimits;
	private Dictionary<ResourceType,Texture2D> resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private bool AreWeDrawingObjetBar;
	
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15, SCROLL_BAR_WIDTH = 22, BUTTON_SPACING = 7;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUILD_IMAGE_PADDING = 8;
	private const int OBJECT_BAR_HEIGHT = 200;
	private const int OBJECT_BAR_MARGIN = 20;
	/*** Game Engine Methods ***/
	
	void Start () {
		player = transform.root.GetComponent<Player>();
		resourceValues = new Dictionary<ResourceType, int>();
		resourceLimits = new Dictionary<ResourceType, int>();
		resourceImages = new Dictionary<ResourceType, Texture2D>();
		AreWeDrawingObjetBar = true;
		for(int i=0; i<resources.Length; i++) {
			switch(resources[i].name) {
				case "Money":
					resourceImages.Add(ResourceType.Money, resources[i]);
					resourceValues.Add(ResourceType.Money, 0);
					resourceLimits.Add(ResourceType.Money, 0);
					break;
				case "Power":
					resourceImages.Add(ResourceType.Power, resources[i]);
					resourceValues.Add(ResourceType.Power, 0);
					resourceLimits.Add(ResourceType.Power, 0);
					break;
				default: break;
			}
		}
		Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
		for(int i=0; i<resourceHealthBars.Length; i++) {
			switch(resourceHealthBars[i].name) {
			case "ore":
				resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
				break;
			default: break;
			}
		}
		ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);
		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
		SetCursorState(CursorState.Select);
	}

	void OnGUI () {
		//we only want to draw a GUI for human players
		if(player.human) {
			DrawOrdersBar();
			DrawResourceBar();
			DrawObjectBar();
			//call last to ensure that the custom mouse cursor is seen on top of everything
			DrawMouseCursor();
		}
	}
	
	/*** Public methods for interacting with the HUD ***/
	
	public bool MouseInBounds() {
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-right of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >=0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		bool insideHeight2;
		if(AreWeDrawingObjetBar){
			insideHeight2 = mousePos.y >=0 && mousePos.y >=  OBJECT_BAR_HEIGHT;
		}else{
			insideHeight2 = true;
		}
		return insideWidth && insideHeight && insideHeight2;
	}
	
	public Rect GetPlayingArea() {
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT );
	}
	
	public void SetCursorState(CursorState newState) {
		if(activeCursorState != newState) previousCursorState = activeCursorState;
		activeCursorState = newState;
		switch(newState) {
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		case CursorState.RallyPoint:
			activeCursor = rallyPointCursor;
			break;
		case CursorState.Plus:
			activeCursor = leftCursor;
			break;
		case CursorState.Minus:
			activeCursor = rallyPointCursor;
			break;
		default: break;
		}
	}
	
	public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}
	
	public CursorState GetCursorState() {
		return activeCursorState;
	}
	
	public CursorState GetPreviousCursorState() {
		return previousCursorState;
	}
	
	/*** Private Worker Methods ***/
	
	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH-BUILD_IMAGE_WIDTH,RESOURCE_BAR_HEIGHT,ORDERS_BAR_WIDTH+BUILD_IMAGE_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH+SCROLL_BAR_WIDTH,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");
		string selectionName = "";
		//if(player.selectedObjects.IfAny()) {
			/*selectionName = player.SelectedObject.objectName;
			if(player.SelectedObject.IsOwnedBy(player)){
				//reset slider value if the selected object has changed
				if(lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
				DrawActions(player.SelectedObject.GetActions());
				//store the current selection
				lastSelection = player.SelectedObject;
				Building selectedBuilding = lastSelection.GetComponent<Building>();
				if(selectedBuilding) {
					DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}*/
		//}
		if(!selectionName.Equals("")) {
			int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
			int topPos = buildAreaHeight + BUTTON_SPACING;
			GUI.Label(new Rect(leftPos,topPos,ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), selectionName);
		}
		GUI.EndGroup();
	}
	
	void DrawObjectBar(){
			GUI.skin = ObjectSkin;
			GUI.BeginGroup(new Rect(0 , Screen.height-OBJECT_BAR_HEIGHT , Screen.width, Screen.height));
			GUI.Box(new Rect(0 , 0 , Screen.width, OBJECT_BAR_HEIGHT),"");
			if(player.selectedObjects.IfAny()){
				if(player.selectedObjects.IfAny()){
					if(player.selectedObjects.Count()!=0){
						int numberOfCategories = player.selectedObjects.NumberOfClassObjects.Count;	
						List<string> categoryname = new List<string>(player.selectedObjects.NumberOfClassObjects.Keys);
						GUI.skin = ObjectCatSkin;

						int NumberOfPerviouslyManagedObjects = 0;
						int GeneralWidth = 0;
						int MaxAmountOfBoxes = 0;
						int WidthArleadyTaken = 0; //This variable tels us how much more on right should the next box be, cuse on his left there is another box
						for(int j = 0;j<categoryname.Count;j++){ // Wir counting how wide the boxes are gonna be!
							GeneralWidth += 20;//margin left
							for(int i=0; i<player.selectedObjects.NumberOfClassObjects[categoryname[j]]; i++){
								GeneralWidth += 64 + 20;// Width of image + margin
								if(GeneralWidth>Screen.width){
									MaxAmountOfBoxes = i;
								}
							}
						}
						
						
					
						for(int j=0; j<categoryname.Count;j++){ // My idea is that categoryname.Count gives the number of diffrent categories of World Objects (Building, Unit etc), and it is to be stored in diffrent boxes! 

							int LeftPos = (Screen.width-GeneralWidth)/2+WidthArleadyTaken;//Screen.width/(numberOfCategories)/2-200+(j*400); //width of box is 400, so half is 200	
							int width = 20 + (BUILD_IMAGE_WIDTH+20)*player.selectedObjects.NumberOfClassObjects[categoryname[j]];//sets width of box depending on amount of 
							WidthArleadyTaken += width;
								GUI.Box(new Rect (LeftPos,14,width,100), categoryname[j]);		
						
								for(int i=0; i<player.selectedObjects.NumberOfClassObjects[categoryname[j]]; i++){//writing box for every object from a category
									WorldObject obj = player.selectedObjects.GiveObject(NumberOfPerviouslyManagedObjects);// object which we are going to write box for
									NumberOfPerviouslyManagedObjects++; //Objects are arleady Sorted out according to groups, so we only have to select next and next object
									//////////////////////////////////////////////////////////////
								 	Rect pos = GetImagePos(i/8, i%8, LeftPos,14);
									Texture2D image = obj.buildImage;
							
									if(GUI.Button( pos, image)){
										if(Event.current.button == 0){//Left button
											player.selectedObjects.MakeSelectionOfOne(obj);
										}
										else if (Event.current.button == 1){
											//player.selectedObjects.RemoveOne(obj);
											obj.SetSelection(false, player.hud.GetPlayingArea());
										}
										else if (Event.current.button == 2){
											
											//UserInput.MoveCameraToObject(obj);
											player.GetComponent<UserInput>().MoveCameraToPosition(obj.transform.position );
									
	
										}
									}
								
									GUI.Label ( new Rect(pos.x, pos.y+64-20,BUILD_IMAGE_WIDTH, 20) , obj.objectName);
									Rect HealthBar = new Rect(pos.x+8, pos.y+10, BUILD_IMAGE_WIDTH-16,5);
									obj.DrawHealthBar(HealthBar,"");
								}
						}

					}
				}
			}
					
			GUI.EndGroup();
		}
	
	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		//define the area to draw the actions inside
		GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH,0,ORDERS_BAR_WIDTH,buildAreaHeight));
		//draw scroll bar for the list of actions if need be
		if(numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
		//display possible actions as buttons and handle the button click for each
		for(int i=0; i<numActions; i++) {
			int column = i % 2;
			int row = i / 2;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				//create the button and handle the click of that button
				if(GUI.Button(pos, action)) {
					//if(player.SelectedObjects.Count()) player.SelectedObjects.PerformAction(actions[i]); ///////////////////TODO
				}
			}
		}
		GUI.EndGroup();
	}
	
	private int MaxNumRows(int areaHeight) {
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}
	
	private Rect GetButtonPos(int row, int column) {
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left,top,BUILD_IMAGE_WIDTH,BUILD_IMAGE_HEIGHT);
	}
	
	private Rect GetImagePos(int row, int column, int initialLeftPosition, int initialTopPosition){
				int left = initialLeftPosition + OBJECT_BAR_MARGIN + (64 + OBJECT_BAR_MARGIN)*(column );
				int top =  initialTopPosition + OBJECT_BAR_MARGIN + (64 + OBJECT_BAR_MARGIN)*( row );
				//Debug.Log("initialTopPosition "+initialTopPosition+"OBJECT_BAR_MARGIN"+OBJECT_BAR_MARGIN+"row"+row);
				return new Rect(left,top,BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}
				
	
	private void DrawSlider(int groupHeight, float numRows) {
		//slider goes from 0 to the number of rows that do not fit on screen
		sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight),sliderValue,0.0f,numRows-MaxNumRows(groupHeight));
	}
	
	private Rect GetScrollPos(int groupHeight) {
		return new Rect(BUTTON_SPACING,BUTTON_SPACING,SCROLL_BAR_WIDTH,groupHeight - 2 * BUTTON_SPACING);
	}
	
	private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
		for(int i=0; i<buildQueue.Length; i++) {
			float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING,topPos,BUILD_IMAGE_WIDTH,BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos,ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos,buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			if(i==0) {
				//shrink the build mask on the item currently being built to give an idea of progress
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING,topPos,width,height),buildMask);
		}
	}
	
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
			building.Sell();
		}
		if(building.hasSpawnPoint()) {
			leftPos += width + BUTTON_SPACING;
			if(GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage)) {
				if(activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) SetCursorState(CursorState.RallyPoint);
				else {
					//dirty hack to ensure toggle between RallyPoint and not works ...
					SetCursorState(CursorState.PanRight);
					SetCursorState(CursorState.Select);
				}
			}
		}
	}
	
	private void DrawResourceBar() {
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
		
		int topPos = 4, iconLeft = 4, textLeft = 20;
		DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
		
		GUI.EndGroup();
	}
	
	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}
	
	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
		if(mouseOverHud) {
			Screen.showCursor = true;
		} else {
			Screen.showCursor = false;
			if(!player.IsFindingBuildingLocation()) {
				GUI.skin = mouseCursorSkin;
				GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height));
				UpdateCursorAnimation();
				Rect cursorPosition = GetCursorDrawPosition();
				GUI.Label(cursorPosition, activeCursor);
				GUI.EndGroup();
			}
		}
	}
	
	private void UpdateCursorAnimation() {
		//sequence animation for cursor (based on more than one image for the cursor)
		//change once per second, loops through array of images
		if(activeCursorState == CursorState.Move) {
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
		} else if(activeCursorState == CursorState.Attack) {
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
		} else if(activeCursorState == CursorState.Harvest) {
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
		}
	}
	
	private Rect GetCursorDrawPosition() {
		//set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
		//adjust position base on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		} else if(activeCursorState == CursorState.RallyPoint) topPos -= activeCursor.height;
		
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}
}
