

var m_oScene, m_oGizmoScene, m_oRenderer, m_oOrbitRaycaster;
var m_oEventList;

var m_oOrbitCamera;

var m_fGridDimensions = 1000.0;
var m_fGridSpacing = 50.0;
var m_oGridMesh, m_oAxisMesh;
var m_oCircleMesh;

var m_oTranslationGizmo;

var m_oGLModels;
var m_oShip;

var m_oSelectedModels;

var manager, loader, onProgress, onError;

var modelIndex = 0;
var modelList = [];

let currObjPath = "";
let currMtlPath = "";
let initedCanvas = false;

//#endregion

//init();
//Render();

function display(objPath, mtlPath) {
    currObjPath = objPath;
    currMtlPath = mtlPath;

    if (!initedCanvas) {
        init();
        initedCanvas = true;
        Render();
    } else {
        ClearScene();
        LoadModel();
    }

}


//Initializer
function init() {
    var scene3d = document.getElementById("three-canvas");


    m_oScene = new THREE.Scene();
    m_oGizmoScene = new THREE.Scene();

    m_oEventList = new Array();
    m_oGLModels = new Array();

    window.addEventListener('resize', onWindowResize, false);

    m_oOrbitCamera = new PerspectiveCamera();
    m_oOrbitCamera.UpdateTransform();
    m_oEventList.push(m_oOrbitCamera);
    m_oScene.add(m_oOrbitCamera.getGLCamera());

    m_oOrbitRaycaster = new Raycaster(m_oOrbitCamera.getGLCamera());
    m_oEventList.push(m_oOrbitRaycaster);

    CreateGrid();
    CreateCircle();
    CreateGizmos();

    manager = new THREE.LoadingManager();
    manager.onProgress = function (item, loaded, total) {
        console.log(item, loaded, total);
    };
    loader = new THREE.OBJLoader(manager);

    onProgress = function (xhr) {
        if (xhr.lengthComputable) {
            var percentComplete = xhr.loaded / xhr.total * 100;
            console.log(Math.round(percentComplete, 2) + '% downloaded');
            //document.getElementById("load-progress").innerHTML = "LoadProgress: " + percentComplete + "%";
        }
    };

    onError = function (xhr) {
    };

    THREE.Loader.Handlers.add( /\.jpg$/i, new THREE.TextureLoader() );
    THREE.Loader.Handlers.add( /\.png$/i, new THREE.TextureLoader() );

    modelIndex = 0;
    modelList = [
        "https://storage.googleapis.com/realitycheck/e5b5ea7c27cd293b4295a69d2e324ad5/bugatti.obj",
        "ACES/acesjustforroomshow.obj",
        "Rocket Launcher/rocketlauncher.obj",
        "GlobalHawk/GlobalHawkOBJ.obj",
        "r8_gt_obj/r8_gt_obj.obj",
        "JunoOBJ/Juno.obj",
        "Paris/Paris2010_0.obj",
    ];

    LoadModel();

    //#endregion

    RegisterModels();

    for (var i = 0; i < m_oGLModels.length; i++)
    {
        m_oEventList.push(m_oGLModels[i]);
    }

    m_oSelectedModels = new Array();

    m_oRenderer = new THREE.WebGLRenderer();
    //m_oRenderer.setSize(window.innerWidth, window.innerHeight);
    m_oRenderer.setSize(scene3d.offsetWidth, scene3d.offsetHeight);
    m_oRenderer.setClearColor(0xeaeaea, 1.0);
    m_oRenderer.autoClear = false;
    m_oRenderer.domElement.style.position = "absolute";
    m_oRenderer.domElement.style.left = "0px";
    m_oRenderer.domElement.style.top = "0px";
    m_oRenderer.domElement.addEventListener('mousedown', onMouseDown);
    m_oRenderer.domElement.addEventListener('mouseup', onMouseUp);
    m_oRenderer.domElement.addEventListener('mousemove', onMouseMove);
    m_oRenderer.domElement.addEventListener('mouseout', onMouseOut);
    m_oRenderer.domElement.addEventListener('mousewheel', onMouseWheel);
    document.addEventListener('keydown', onKeyDown);
    document.oncontextmenu = function(event){
		event.preventDefault();
	    event.stopPropagation();
	    return false;
	};

    var directionalLight = new THREE.DirectionalLight(0xFFFFFF);
    directionalLight.position.set(-40, -60, 50);
    m_oScene.add(directionalLight);

    var directionalLight2 = new THREE.DirectionalLight(0xFFFFFF);
    directionalLight2.position.set(40, 60, -50);
    m_oScene.add(directionalLight2);

    scene3d.appendChild(m_oRenderer.domElement);
}

//#region RenderLoop
function Render() {

    requestAnimationFrame(Render);

    var cirlcePos = m_oOrbitCamera.getOrigin();
    m_oCircleMesh.position.set(cirlcePos.x, cirlcePos.y, cirlcePos.z);

    m_oSelectedModels = new Array();
    for (var i = 0; i < m_oGLModels.length; i++)
    {
        if (m_oGLModels[i].getIsSelected())
        {
            m_oSelectedModels.push(m_oGLModels[i]);
        }
    }

    for (var i = 0; i < m_oGLModels.length; i++) {
        m_oGLModels[i].UpdateColors();
    }

    m_oRenderer.clear();
    m_oRenderer.render(m_oScene, m_oOrbitCamera.getGLCamera());

    for (var i = 0; i < m_oGLModels.length; i++)
    {
        GizmoTransforms(m_oGLModels[i]);
    }

    if (m_oSelectedModels.length > 0)
    {
        UpdateGizmos(m_oOrbitCamera);

        m_oRenderer.clearDepth();
        m_oRenderer.render(m_oGizmoScene, m_oOrbitCamera.getGLCamera());
    }
}

function onWindowResize() {

    m_oOrbitCamera.Resize();
    // TODO fix
    //m_oRenderer.setSize(window.innerWidth, window.innerHeight);
}

//#endregion

//#region RenderTools

function CreateGrid()
{
    m_fGridDimensions /= 2;

    var numberOfLines = Math.ceil((m_fGridDimensions / 2.0) / m_fGridSpacing);
    numberOfLines *= 2;

    if (numberOfLines % 2 == 0)
        numberOfLines++;

    numberOfLines *= 2;
    numberOfLines -= 2;

    m_oGridMesh = new THREE.GridHelper(m_fGridDimensions, numberOfLines, new THREE.Color(0.59, 0.59, 0.59), new THREE.Color(0.427, 0.631, 0.894));
    m_oGridMesh.rotation.x = 1.570796;
    m_oScene.add(m_oGridMesh);

    m_oAxisMesh = new THREE.AxisHelper(1000);
    m_oAxisMesh.scale.y = 0;
    m_oAxisMesh.scale.x = 0;
    m_oScene.add(m_oAxisMesh);

    m_fGridDimensions *= 2;
}

function CreateCircle()
{
    var curve = new THREE.EllipseCurve(
        0, 0,
        1, 1,
        0, 2 * Math.PI,
        false, 0
        );

    var path = new THREE.Path(curve.getPoints(50));
    var oCircleGeometry = path.createPointsGeometry(50);
    var oCircleMaterial = new THREE.LineBasicMaterial({ color: new THREE.Color(0, 1, 0) });

    m_oCircleMesh = new THREE.Line(oCircleGeometry, oCircleMaterial);
    var position = m_oOrbitCamera.getOrigin();
    m_oCircleMesh.position.set(position.x, position.y, position.z);
    m_oScene.add(m_oCircleMesh);
}

function CreateGizmos()
{
    m_oTranslationGizmo = new TranslationGizmo();
    m_oGizmoScene.add(m_oTranslationGizmo.getXCone().getGLMesh());
    m_oGizmoScene.add(m_oTranslationGizmo.getYCone().getGLMesh());
    m_oGizmoScene.add(m_oTranslationGizmo.getZCone().getGLMesh());
    m_oGizmoScene.add(m_oTranslationGizmo.getLines());
}

function GizmoTransforms(oModel)
{
    if (m_oSelectedModels.length == 0)
    {
        m_oTranslationGizmo.Reset();

        m_oOrbitCamera.setStaticCamera(false);
    }
    else if (oModel.getIsSelected())
    {
        m_oTranslationGizmo.setPosition(oModel.getGLMesh().position);

        if (m_oTranslationGizmo.getXCone().getIsSelected() || m_oTranslationGizmo.getYCone().getIsSelected() || m_oTranslationGizmo.getZCone().getIsSelected()) {
            m_oOrbitCamera.setStaticCamera(true);
            oModel.Translate(m_oTranslationGizmo, m_oOrbitCamera, m_fGridDimensions);
        }
        else
            m_oOrbitCamera.setStaticCamera(false);
    }
}

function UpdateGizmos(oCamera)
{
    m_oTranslationGizmo.Update(oCamera);
}

function RegisterModels()
{
    var oModelsToRegister = new Array();

    for (var i = 0; i < m_oGLModels.length; i++)
    {
        oModelsToRegister.push(m_oGLModels[i]);
    }

    oModelsToRegister.push(m_oTranslationGizmo.getXCone());
    oModelsToRegister.push(m_oTranslationGizmo.getYCone());
    oModelsToRegister.push(m_oTranslationGizmo.getZCone());

    m_oOrbitRaycaster.SetModels(oModelsToRegister);
}

function AddModel(oModel)
{
    m_oEventList.push(oModel);
    m_oGLModels.push(oModel);

    RegisterModels();
}

function LoadModel()
{
    console.log("OBJ: " + currObjPath);
    console.log("MTL: " + currMtlPath);

    const path = "https://storage.googleapis.com/realitycheck/";

    var objFile = currObjPath;
    var mtlFile = currMtlPath;

    var http = new XMLHttpRequest();
    http.open("GET", path + mtlFile);

    http.onreadystatechange = function(e) {
        //Check if mtl file exists
        if (http.responseText.indexOf("html") > -1)
        {
            var objLoader = new THREE.OBJLoader(manager);
            objLoader.load(path + objFile, function(object) {
                object.traverse(function (child) {
                    if (child instanceof THREE.Mesh) {
                        child.material = newPlaneMaterial();
                    }
                });

                // Do this becuase I have 'z' as up and I don't feel like changing it
                object.rotation.x = 1.570796;
                object.name = "sceneTestModel";
                m_oScene.add(object);

            }, onProgress, onError);
        }
        else
        {
            var mtlLoader = new THREE.MTLLoader(manager);
            mtlLoader.setPath(path);
            mtlLoader.load(mtlFile, function(materials) {
                console.log('mats', materials);
                materials.preload();

                var objLoader = new THREE.OBJLoader(manager);
                objLoader.setMaterials(materials);
                objLoader.load(path + objFile, function(object) {

                    // Do this becuase I have 'z' as up and I don't feel like changing it
                    object.rotation.x = 1.570796;
                    object.name = "sceneTestModel";
                    m_oScene.add(object);

                }, onProgress, onError);

            }, onProgress, onError );
        }
    };

    http.send();
}

function UpdateProperties(objFileFull)
{
    /*
    var lastSlash = objFileFull.lastIndexOf("/");
    var path = objFileFull.substr(0, lastSlash + 1);
    var objFile = objFileFull.substr(lastSlash + 1, objFileFull.length - lastSlash + 1);
    var mtlFile = objFile.replace(".obj", ".mtl");

    var contentRoot = document.getElementById("properties-content");
    contentRoot.innerHTML = "";

    var info = [
        ["File Name", objFile],
        ["File Size", "50MB"],
        ["Color", "some color"],
        ["Tags", "space, ship, NASA, plane"],
        ["Source", "Free3D"]
    ];

    for (var i = 0; i < info.length; i++)
    {
        var header = document.createElement("h3");
        header.id = info[i][0];
        header.innerHTML = info[i][0];
        contentRoot.appendChild(header);
        var details = document.createElement("p");
        details.id = info[i][0] + "-details";
        details.innerHTML = info[i][1];
        contentRoot.appendChild(details);
    }*/
}

/*
function NextModel()
{
    ClearScene();
    modelIndex++;
    modelIndex = modelIndex % modelList.length;
    LoadModel(modelList[modelIndex]);
}

function PreviousModel()
{
    ClearScene();
    modelIndex--;
    if (modelIndex < 0)
    {
        modelIndex = modelList.length - 1;
    }
    modelIndex = modelIndex % modelList.length;
    LoadModel(modelList[modelIndex]);
}*/

function ClearScene()
{
    if (!initedCanvas) return;

    for (var i = 0; i < m_oScene.children.length; i++)
    {
        if (m_oScene.children[i].name == "sceneTestModel")
        {
            m_oScene.remove(m_oScene.children[i]);
            i--;
        }
    }
}

function newPlaneMaterial()
{
    return new THREE.MeshLambertMaterial({ color: 0xFFFFFF, side: THREE.DoubleSide, opacity: 1.0, transparent: true, depthWrite: true });
}

function ToggleModelList()
{
    /*
    var content = document.getElementById("list-content");
    if (content.style.display === "block")
    {
        content.style.display = "none";
    }
    else
    {
        content.style.display = "block";
    }*/
}

function ToggleProperties()
{
    /*
    var content = document.getElementById("properties-content");
    if (content.style.display === "block")
    {
        content.style.display = "none";
    }
    else
    {
        content.style.display = "block";
    }*/
}

//#endregion

//#region InputEvents

function onMouseDown(event) {
    event.preventDefault();

    for (var i = 0; i < m_oEventList.length; i++)
    {
        m_oEventList[i].onMouseDown(event);
    }
}

function onMouseUp(event) {
    event.preventDefault();

    for (var i = 0; i < m_oEventList.length; i++) {
        m_oEventList[i].onMouseUp(event);
    }
}

function onMouseMove(event) {
    event.preventDefault();

    for (var i = 0; i < m_oEventList.length; i++) {
        m_oEventList[i].onMouseMove(event);
    }
}

function onMouseOut(event) {
    event.preventDefault();

    for (var i = 0; i < m_oEventList.length; i++) {
        m_oEventList[i].onMouseOut(event);
    }
}

function onMouseWheel(event) {
    event.preventDefault();

    for (var i = 0; i < m_oEventList.length; i++) {
        m_oEventList[i].onMouseWheel(event);
    }
}

function onKeyDown(event) {

}

// shared state of current loaded asset
const asset_store = {
    debug: true,
    state: {
        asset: {},
        assetList: {},
        searchResults: {},
        searchQuery: "",
        uuid: null
    },
    setAsset (a) {
        if (this.debug) console.log('changed loaded asset: ', a);
        this.state.asset = a
    },
    setSearchResults (sr) {
        if (this.debug) console.log('changed search results: ', sr);
        this.state.searchResults = sr;
    },
    getSearchAssets () {
        if (this.debug) console.log('mapping search results');
        console.log(this.state);
        if (this.state.searchResults['hits'] !== undefined) {
            return this.state.searchResults['hits'].map(a => a['asset']);
        }
       return {}
    },
    getBrowseAssets () {
        if (this.debug) console.log('mapping search results');

        if (this.state.assetList['items'] !== undefined) {
            return this.state.assetList['items'];
        }
        return []
    }

};

var bus = new Vue();


/*
const app_models = new Vue({
    el: '#app-models',
    data: {
        assets: []
    },
    methods: {
        fetchData: () => {
            this.$http.get('/models')
                .then(function (response) {
                    // just load first
                    asset_store.setAsset(response.data[0]);
                }, function (err) {
                    console.log(err);
                });
        }
    },
    mounted: () => {
        //this.fetchData();
    }
});

// see: https://github.com/josdejong/jsoneditor
const app_asset_properties = new Vue({
    el: '#app-asset-properties',
    data: {
        asset: asset_store.state,
        container: null,
        options: {},
        editor: null
    },
    methods: {
        load: () => {

        },
        getJson: () => {
            return this.editor.get();
        },
        updateJson: () => {
            this.$http.put('/models', this.a)
                .then(function (response) {
                    // just load first
                    asset_store.setAsset(response.data[0]);
                }, function (err) {
                    console.log(err);
                });
        }
    },
    mounted: () => {
        this.container = document.getElementById("jsoneditor");
        this.editor = new JSONEditor(this.container, this.options);

        let json = {
            "Array": [1, 2, 3],
            "Boolean": true,
            "Null": null,
            "Number": 123,
            "Object": {"a": "b", "c": "d"},
            "String": "Hello World"
        };

        this.editor.set(json);
        console.log("set editor");
    }
});
*/

Vue.component('asset-properties', {
    template: `
        <div>
            <select class="form-control form-control-sm" id="objSelect">
              <option v-for="o in objOptions">{{o}}</option>
            </select>
            <select class="form-control form-control-sm" id="mtlSelect">
              <option v-for="m in mtlOptions">{{m}}</option>
            </select>
            
            <button @click="loadScene()" type="button" class="btn btn-secondary" style="width: 100%">Load</button>
            <button @click="setPrimaryFiles()" type="button" class="btn btn-danger" style="width: 100%">Set as Primary</button>
            
            <div style="height: 50px"></div>
            <div style="height: 50px"></div>
            
            <button @click="saveJson()" type="button" class="btn btn-danger" style="width: 100%">Save Json</button>
            <div ref="jsoneditor" style="height: 100%"></div>
            
        </div>`,

    data: () => {
        return {
            jsoneditor: null,
            options: {},
            isLoading: false,
            shared_state: asset_store,
            objOptions: [],
            mtlOptions: []
        }
    },
    methods: {
        updateJson() {
            console.log(this.shared_state);
            this.jsoneditor.set(this.shared_state.state.asset);

            this.indexed = this.shared_state.state.asset['allow_indexing'];

            // TODO sort by primary
            // add primary first
            this.objOptions = this.shared_state.state.asset['archive']['files'].filter(f => f.includes('.obj'));
            this.mtlOptions = this.shared_state.state.asset['archive']['files'].filter(f => f.includes('.mtl'));
        },
        loadScene() {
            const oe = document.getElementById("objSelect");
            const om = document.getElementById("mtlSelect");

            const obj = oe.options[oe.selectedIndex].text;
            const mtl = om.options[om.selectedIndex].text;
            display(obj, mtl);
        },
        setPrimaryFiles() {
            const oe = document.getElementById("objSelect");
            const om = document.getElementById("mtlSelect");

            const obj = oe.options[oe.selectedIndex].text;
            const mtl = om.options[om.selectedIndex].text;

            axios.put('/api/v2/asset/' + this.shared_state.state.uuid + '/primary', {'obj': obj, 'mtl': mtl})
                .then((response) => {
                    console.log("successfully set primary obj and mtl");
                    this.shared_state.state.asset = response.data;
                    this.updateJson();
                })
                .catch((err) => {
                    console.log(err);
                });
        },
        saveJson() {
            axios.put('/api/v2/asset/' + this.shared_state.state.uuid, this.jsoneditor.get())
                .then((response) => {
                    console.log("successfully updated properties");
                    this.shared_state.state.asset = response.data;
                    this.updateJson();
                })
                .catch((err) => {
                    console.log(err);
                });
        }
    },
    mounted() {
        this.jsoneditor = new JSONEditor(this.$refs.jsoneditor, this.options, this.shared_state.state.asset);

        bus.$on('load-asset', this.updateJson);
    }

});

Vue.component('asset-card', {
    props: ['uuid', 'name', 'update_time', 'indexed'],
    template: `
    <div class="col-xs-6 col-sm-4 col-lg-3" >
      <div class="thumbnail">
        <img src="http://placehold.it/350x150">
        
        <div class="caption">
          <h5 v-text="name"></h5>
          <i v-if="indexed" class="far fa-check-circle"></i>
          
        
          <!-- <small class="text-muted">Last updated {{update_time}}</small> -->

        </div>
        </div>
    </div>
    `,

    data: () => {
        return {

        }
    }
});

Vue.component('list-asset-card', {
    template: `
    <div class="flex-row row">
        <div class="card-deck">
            <asset-card v-for="a in shared_store.getSearchAssets()" v-bind:key="a.uuid" v-bind:name="a.name" v-on:click.native="handleClick(a)"></asset-card>
        </div>
    </div>
    `,
    data: () => {
        return {
            shared_store: asset_store
        }
    },
    methods: {
        handleClick (a) {
            console.log('click ' + a.name + ' ' + a.uuid);
            if (a.uuid !== this.shared_store.state.uuid) {
                ClearScene();
            }
            this.shared_store.state.uuid = a.uuid;
            this.$emit('load-asset')
        }
    }
});

Vue.component('search-bar', {
    template: `
        <input @input="handleKey($event.target.value)" :value="term" class="form-control form-control-dark w-100" placeholder="Search" aria-label="Search" type="text">
    `,
    data: () => {
        return {
            term: asset_store.state.searchQuery
        }

    },
    methods: {
        handleKey(term) {
            asset_store.state.searchQuery = term;
            console.log(term);
            this.$emit('trigger-search')
        }
    }


});

Vue.component('search-results', {
    template: `
        <div>{{shared_state.searchResults.count}} results in {{shared_state.searchResults.time}} ms
            <list-asset-card v-on:load-asset="loadAsset"></list-asset-card>
        </div>
    `,
    data: () => {
        return {
            shared_state: asset_store.state
        }
    },
    methods: {
        loadAsset() {
            this.$emit('load-asset')
        }
    }
});


Vue.component('browsing-asset-card', {
    template: `
    <div class="flex-row row">
        <div class="card-deck">
            <asset-card v-for="a in shared_store.getBrowseAssets()" v-bind:key="a.name" v-bind:name="a.name" v-bind:indexed="a.allow_indexing" v-on:click.native="handleClick(a)"></asset-card>
        </div>
    </div>
    `,
    data: () => {
        return {
            shared_store: asset_store
        }
    },
    methods: {
        handleClick (a) {
            console.log('click ' + a.name + ' ' + a['_id']['$oid']);

            if (a['_id']['$oid'] !== this.shared_store.state.uuid) {
                ClearScene();
            }

            this.shared_store.state.uuid = a['_id']['$oid'];
            this.$emit('load-asset')
        }
    }
});


Vue.component('browsing', {
    template: `
        <div>
            <browsing-asset-card v-on:load-asset="loadAsset"></browsing-asset-card>
        </div>
    `,
    data: () => {
        return {
            shared_state: asset_store.state
        }
    },
    methods: {
        loadAsset() {
            this.$emit('load-asset')
        }
    },
    mounted() {
        axios.get('/api/v2/assets/')
                .then(function (response) {
                    asset_store.state.assetList = response.data;
                })
                .catch(function (err) {
                    console.log(err);
                });
    }
});



new Vue({
    el: '#root',
    methods: {
        handleTabChange() {
            this.tabIndex ++;
        },
        loadModels() {
            this.tabIndex = 1;
            console.log(asset_store.state);

            axios.post('/api/v1/search', {
                'query': asset_store.state.searchQuery,
                'pageNumber': 1,
                'resultPerPage': 10
            }).then(function (response) {
                asset_store.state.searchResults = response.data;


                    // just load first
                    console.log(response.data);
                    //asset_store.setAsset(response.data[0]);
                })
                .catch(function (err) {
                    console.log(err);
                });
        },
        loadAsset() {
            this.tabIndex = 2;
            console.log("requesting");

            axios.get('/api/v2/asset/' + asset_store.state.uuid)
                .then(function (response) {
                    asset_store.state.asset = response.data;
                    bus.$emit('load-asset');
                    console.log(response.data);
                })
                .catch(function (err) {
                    console.log(err);
                });
        }
    },
    data () {
        return {
            tabIndex: 0
        }
    },
    mounted() {
        //init();
        //Render();
    }
});