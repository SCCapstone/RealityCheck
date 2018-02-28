
function Raycaster(oCamera)
{
    //#region Constructor

    var m_oGLRaycaster = new THREE.Raycaster();
    var m_oMouse = new THREE.Vector2(0, 0);

    var Entites = new Array();
    var m_oCamera = oCamera;

    var m_bScanning = false;
    var m_bDoEntityRaycasting = false;

    var m_oTempGLModel;
    var m_oTempGLGizmoModel;
    var m_bGizmoIsSelected = false;

    var m_bMouseRightDown = false;
    var m_bMouseLeftDown = false;
    
    //#endregion

    var Raytrace = function(bIsMouseDown)
    {
        m_oGLRaycaster.setFromCamera(m_oMouse, m_oCamera);
        
        var list = new Array();

        for (var i = 0; i < Entites.length; i++)
        {
            list.push(Entites[i].getGLMesh());
        }
        
        var intersections = m_oGLRaycaster.intersectObjects(list);

        if (intersections.length > 0)
        {
            for (var i = 0; i < Entites.length; i++)
            {
                for (var j = 0; j < intersections.length; j++)
                {
                    if (Entites[i].getGLMesh() === intersections[0].object && Entites[i].getName() != "Gizmo")
                    {
                        if (m_bScanning === false) {
                            Entites[i].setIsSelected(true);
                        }
                        else {
                            Entites[i].setIsHighlighted(true);
                        }
                    }
                    else if (Entites[i].getGLMesh() === intersections[j].object && Entites[i].getName() === "Gizmo")
                    {
                        if (bIsMouseDown === true) {
                            Entites[i].setIsSelected(true);
                        }
                        else {
                            Entites[i].setIsHighlighted(true);
                        }
                    }
                }
            }
        }
        else
        {
            if (bIsMouseDown && m_bGizmoIsSelected === false)
            {
                for (var i = 0; i < Entites.length; i++)
                {
                    Entites[i].setIsHighlighted(false);
                    Entites[i].setIsSelected(false);
                }
            }
        }
       
    };

    Raycaster.prototype.SetModels = function(oGLModels)
    {
        Entites = oGLModels;
    };

    Raycaster.prototype.onMouseDown = function (event)
    {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;

        if (event.button == 0)
        {
            m_bMouseLeftDown = true;
        }
        if (event.button == 2)
        {
            m_bMouseRightDown = true;
        }

        if (m_bMouseLeftDown)
        {
            m_oTempGLModel = null;
            var bModelIsSelected = false;
            m_bGizmoIsSelected = false;
            
            //=================================================================================

            for (var i = 0; i < Entites.length; i++)
            {
                if (Entites[i].getName() === "Gizmo" && Entites[i].getIsHighlighted() === true)
                {
                    m_bGizmoIsSelected = true;
                }

                if (Entites[i].getName() != "Gizmo" && Entites[i].getIsSelected() === true)
                {
                    bModelIsSelected = true;
                    m_oTempGLModel = Entites[i];
                }
            }

            //=================================================================================

            for (var i = 0; i < Entites.length; i++)
            {
                if (m_bGizmoIsSelected && bModelIsSelected)
                {
                    if (Entites[i] != m_oTempGLModel)
                    {
                        Entites[i].setIsSelected(false);
                    }
                    Entites[i].setIsHighlighted(false);
                }

                if (m_bGizmoIsSelected === false)
                {
                    Entites[i].setIsSelected(false);
                    Entites[i].setIsHighlighted(false);
                }
            }

            //=================================================================================

            if (bModelIsSelected && m_bGizmoIsSelected)
            {
                m_bScanning = true;
            }
            else
            {
                m_bScanning = false;
            }

            m_bDoEntityRaycasting = true;
            Raytrace(true);
        }
    };

    Raycaster.prototype.onMouseUp = function (event)
    {
        if (m_bDoEntityRaycasting && m_bMouseRightDown == false)
        {
            for (var i = 0; i < Entites.length; i++)
            {
                if (Entites[i].getName() === "Gizmo")
                {
                    Entites[i].setIsSelected(false);
                    Entites[i].setIsHighlighted(false);
                }
            }

            m_bScanning = false;
        }

        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;
    };

    Raycaster.prototype.onMouseMove = function (event)
    {
        m_oMouse.x = (event.clientX / window.innerWidth) * 2 - 1;
        m_oMouse.y = -(event.clientY / window.innerHeight) * 2 + 1;

        for (var i = 0; i < Entites.length; i++)
        {
            Entites[i].setIsHighlighted(false);
        }

        if (m_bMouseRightDown)
        {
            m_bDoEntityRaycasting = false;
        }
        else
        {
            m_bDoEntityRaycasting = true;
            m_bScanning = true;
            Raytrace(false);
        }
    };

    Raycaster.prototype.onMouseOut = function (event)
    {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;
    };

    Raycaster.prototype.onMouseWheel = function (event)
    {
        //Not Used
    };
}