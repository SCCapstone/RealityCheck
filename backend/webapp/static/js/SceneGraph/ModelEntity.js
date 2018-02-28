

function ModelEntity(oGLGeometry, oGLMaterial)
{
    //#region Constructor

    this.m_oGLMesh = new THREE.Mesh(oGLGeometry, oGLMaterial);
    this.m_oMaterialColor = new THREE.Color(this.m_oGLMesh.material.color.r, this.m_oGLMesh.material.color.g, this.m_oGLMesh.material.color.b);

    this.m_bIsSelected = false;
    this.m_bIsHighlighted = false;

    var tempBox = new THREE.Box3().setFromObject(this.m_oGLMesh);
    this.m_oBoundingBox = new BoundingBox(tempBox.min, tempBox.max, this.m_oGLMesh);
    this.m_oName = "";

    var m_vCameraPosition = new THREE.Vector3();
    var m_fCameraDistanceToOrigin = 0;
    var m_fMainGridDimensions = 0;

    var m_bMouseRightDown = false;
    var m_bMouseLeftDown = false;

    this.m_oOldMousePosition = new THREE.Vector2(0, 0);

    this.m_bTranslate_X = false;
    this.m_bTranslate_Y = false;
    this.m_bTranslate_Z = false;
    
    //#endregion

    //#region Methods

    ModelEntity.prototype.UpdateBoundingBox = function()
    {
        this.m_oBoundingBox.getBoxMesh().position.set(this.m_oGLMesh.position.x, this.m_oGLMesh.position.y, this.m_oGLMesh.position.z);
        this.m_oBoundingBox.getBoxMesh().scale.set(this.m_oGLMesh.scale.x, this.m_oGLMesh.scale.y, this.m_oGLMesh.scale.z);
        this.m_oBoundingBox.getBoxMesh().rotation.set(this.m_oGLMesh.rotation.x, this.m_oGLMesh.rotation.y, this.m_oGLMesh.rotation.z);
    };

    ModelEntity.prototype.UpdateColors = function()
    {
        if (this.m_bIsSelected)
        {
            this.m_oGLMesh.material.color.set(0x00ffff);
        }
        else if (this.m_bIsHighlighted)
        {
            this.m_oGLMesh.material.color.set(0xffff00);
        }
        else
        {
            this.m_oGLMesh.material.color.set(this.m_oMaterialColor);
        }
    };

    //#region Transformations

    ModelEntity.prototype.Translate = function(oGizmo, oCamera, oGridDemensions)
    {
        m_vCameraPosition = new THREE.Vector3(oCamera.getGLCamera().position.x - oCamera.getOrigin().x,
                                              oCamera.getGLCamera().position.y - oCamera.getOrigin().y,
                                              oCamera.getGLCamera().position.z - oCamera.getOrigin().z);

        m_fCameraDistanceToOrigin = oCamera.getDistance();

        m_fMainGridDimensions = oGridDemensions / 2;

        if (oGizmo.getXCone().getIsSelected())
        {
            this.m_bTranslate_X = true;
        }
        else
        {
            this.m_bTranslate_X = false;
        }

        if (oGizmo.getYCone().getIsSelected())
        {
            this.m_bTranslate_Y = true;
        }
        else {
            this.m_bTranslate_Y = false;
        }

        if (oGizmo.getZCone().getIsSelected())
        {
            this.m_bTranslate_Z = true;
        }
        else {
            this.m_bTranslate_Z = false;
        }
    };

    //#endregion

    //#region MouseEvents

    ModelEntity.prototype.onMouseDown = function (event) {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;

        if (event.button == 0) {
            m_bMouseLeftDown = true;
        }
        if (event.button == 2) {
            m_bMouseRightDown = true;
        }
    };

    ModelEntity.prototype.onMouseUp = function (event) {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;

        this.m_bTranslate_X = false;
        this.m_bTranslate_Y = false;
        this.m_bTranslate_Z = false;
    };

    ModelEntity.prototype.onMouseMove = function (event) {
        var oMouse = new THREE.Vector2(event.clientX, event.clientY);
        var oDelta = new THREE.Vector2(oMouse.x - this.m_oOldMousePosition.x, oMouse.y - this.m_oOldMousePosition.y);

        if (m_bMouseLeftDown && this.m_bIsSelected)
        {
            if (this.m_bTranslate_X || this.m_bTranslate_Y || this.m_bTranslate_Z)
            {
                if (m_fMainGridDimensions == 0)
                {
                    m_fMainGridDimensions = 1000;
                }

                var fTranslateFactor = m_fCameraDistanceToOrigin * 0.0009;

                var fCameraAngle = Math.atan2(m_vCameraPosition.y, m_vCameraPosition.x) * (180 / Math.PI);
                if (fCameraAngle < 0)
                {
                    fCameraAngle += 360;
                }
                
                var oShift = new THREE.Vector3();

                if (this.m_bTranslate_X)
                {
                    if (175 <= fCameraAngle && fCameraAngle <= 185)
                    {
                        oShift = new THREE.Vector3(-oDelta.y * fTranslateFactor, 0, 0);
                    }
                    else if ((355 <= fCameraAngle && fCameraAngle <= 360) || (0 < fCameraAngle && fCameraAngle <= 5))
                    {
                        oShift = new THREE.Vector3(oDelta.y * fTranslateFactor, 0, 0);
                    }
                    else if (m_vCameraPosition.y < 0)
                    {
                        oShift.add(new THREE.Vector3(oDelta.x * fTranslateFactor, 0, 0));
                    }
                    else
                    {
                        oShift.add(new THREE.Vector3(-oDelta.x * fTranslateFactor, 0, 0));
                    }

                    this.m_oGLMesh.position.add(oShift);
                }
                else if (this.m_bTranslate_Y)
                {
                    if (85 <= fCameraAngle && fCameraAngle <= 95)
                    {
                        oShift = new THREE.Vector3(0, oDelta.y * fTranslateFactor, 0);
                    }
                    else if (265 <= fCameraAngle && fCameraAngle <= 275)
                    {
                        oShift = new THREE.Vector3(0, -oDelta.y * fTranslateFactor, 0);
                    }
                    if (m_vCameraPosition.x < 0)
                    {
                        oShift.add(new THREE.Vector3(0, -oDelta.x * fTranslateFactor, 0));
                    }
                    else
                    {
                        oShift.add(new THREE.Vector3(0, oDelta.x * fTranslateFactor, 0));
                    }

                    this.m_oGLMesh.position.add(oShift);
                }
                else if (this.m_bTranslate_Z)
                {
                    this.m_oGLMesh.position.add(new THREE.Vector3(0, 0, -oDelta.y * fTranslateFactor));
                }

                if (this.m_oGLMesh.position.x > m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(m_fMainGridDimensions, this.m_oGLMesh.position.y, this.m_oGLMesh.position.z);
                }
                if (this.m_oGLMesh.position.x < -m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(-m_fMainGridDimensions, this.m_oGLMesh.position.y, this.m_oGLMesh.position.z);
                }

                if (this.m_oGLMesh.position.y > m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(this.m_oGLMesh.position.x, m_fMainGridDimensions, this.m_oGLMesh.position.z);
                }
                if (this.m_oGLMesh.position.y < -m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(this.m_oGLMesh.position.x, -m_fMainGridDimensions, this.m_oGLMesh.position.z);
                }

                if (this.m_oGLMesh.position.z > m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(this.m_oGLMesh.position.x, this.m_oGLMesh.position.y, m_fMainGridDimensions);
                }
                if (this.m_oGLMesh.position.z < -m_fMainGridDimensions) {
                    this.m_oGLMesh.position.set(this.m_oGLMesh.position.x, this.m_oGLMesh.position.y, -m_fMainGridDimensions);
                }

                this.UpdateBoundingBox();
            }
        }

        this.m_oOldMousePosition = oMouse;
    };

    ModelEntity.prototype.onMouseOut = function (event) {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;
    };

    ModelEntity.prototype.onMouseWheel = function (event) {
        //Not Used
    };

    //#endregion

    //#endregion

    //#region Properties

    ModelEntity.prototype.getGLMesh = function() {
        return this.m_oGLMesh;
    };

    ModelEntity.prototype.setName = function(oName) {
        this.m_oName = oName;
    };

    ModelEntity.prototype.getName = function () {
        return this.m_oName;
    };

    ModelEntity.prototype.getBoundingBox = function() {
        return this.m_oBoundingBox;
    };

    ModelEntity.prototype.getIsHighlighted = function() {
        return this.m_bIsHighlighted;
    };

    ModelEntity.prototype.setIsHighlighted = function(value) {
        if (this.m_bIsHighlighted != value)
        {
            this.m_bIsHighlighted = value;
        }
    };

    ModelEntity.prototype.getIsSelected = function() {
        return this.m_bIsSelected;
    };

    ModelEntity.prototype.setIsSelected = function (value) {
        if (this.m_bIsSelected != value)
        {
            this.m_bIsSelected = value;
        }
    };

    //#endregion
}
