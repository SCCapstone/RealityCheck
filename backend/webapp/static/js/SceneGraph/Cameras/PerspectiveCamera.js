
function PerspectiveCamera()
{
    //#region Fields

    var m_oGLCamera;

    var m_fDistance;
    var m_fZRotation;
    var m_fXYPlaneRotation;
    var m_oOrigin;

    var m_fMouseRotateSensitivity;
    var m_fMousePanSensitivity;

    var m_bMouseRightDown = false;
    var m_bMouseLeftDown = false;

    var m_oOldMousePos = new THREE.Vector2(0, 0);

    var m_bStaticCamera = false;

    //#endregion

    //#region Constructor

    m_fDistance = 200.0;
    m_fZRotation = 225 * (3.14159 / 180.0);
    m_fXYPlaneRotation = 14 * (3.14159 / 180.0);
    m_oOrigin = new THREE.Vector3(0, 0, 0);

    m_oGLCamera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 1, 10000);
    m_oGLCamera.up.set(0, 0, 1);

    //#endregion

    //#region MouseEvents

    PerspectiveCamera.prototype.onMouseDown = function(event) {
       
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
    };
    
    PerspectiveCamera.prototype.onMouseUp = function(event) {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;
    };

    PerspectiveCamera.prototype.onMouseMove = function(event) {
        
        var mousePos = new THREE.Vector2(event.clientX, event.clientY);
        var deltaPos = new THREE.Vector2(mousePos.x - m_oOldMousePos.x, mousePos.y - m_oOldMousePos.y);

        if (m_bStaticCamera === false) {

            if (m_bMouseRightDown) {
                m_fMouseRotateSensitivity = 0.3;

                m_fXYPlaneRotation -= (m_fMouseRotateSensitivity * deltaPos.y * (1 * (Math.PI / 180)));
                m_fZRotation -= (m_fMouseRotateSensitivity * m_oGLCamera.aspect * -deltaPos.x * (1 * (Math.PI / 180)));

                var fMaxRadians = 85 * (Math.PI / 180);

                if (m_fXYPlaneRotation > fMaxRadians)
                    m_fXYPlaneRotation = fMaxRadians;

                if (m_fXYPlaneRotation < -fMaxRadians)
                    m_fXYPlaneRotation = -fMaxRadians;
            }
            else if (m_bMouseLeftDown) {
                var forwardVector = new THREE.Vector3(m_oOrigin.x - m_oGLCamera.position.x,
                                                      m_oOrigin.y - m_oGLCamera.position.y,
                                                      m_oOrigin.z - m_oGLCamera.position.z);
                forwardVector.z = 0;
                forwardVector.normalize();

                var rightVector = new THREE.Vector3();
                rightVector.cross(forwardVector, new THREE.Vector3(0, 0, 1));

                var x = m_fMousePanSensitivity * (-deltaPos.x * rightVector.x + deltaPos.y * forwardVector.x);
                var y = m_fMousePanSensitivity * (-deltaPos.x * rightVector.y + deltaPos.y * forwardVector.y);
                var z = m_fMousePanSensitivity * (-deltaPos.x * rightVector.z + deltaPos.y * forwardVector.z);

                m_oOrigin.add(new THREE.Vector3(x, y, z));
            }
        }

        this.UpdateTransform();

        m_oOldMousePos = new THREE.Vector3(mousePos.x, mousePos.y);
    };

    PerspectiveCamera.prototype.onMouseOut = function(event) {
        m_bMouseLeftDown = false;
        m_bMouseRightDown = false;
    };

    PerspectiveCamera.prototype.onMouseWheel = function(event) {

        var delta = event.wheelDelta / 12;
        var mult = m_fDistance * 0.015;

        m_fMousePanSensitivity = m_fDistance * 0.0025;

        if (m_fMousePanSensitivity > 1)
            m_fMousePanSensitivity = 1;

        if (m_fDistance >= 1.5 && m_fDistance <= 10000)
        {
            m_fDistance -= mult * delta;
        }
        else if (m_fDistance < 1.5)
        {
            m_fDistance = 1.5;
        }
        else
        {
            m_fDistance = 10000;
        }

        this.UpdateTransform();
    };

    //#endregion

    PerspectiveCamera.prototype.UpdateTransform = function() {
        var t = m_fDistance * Math.cos(m_fXYPlaneRotation);
        var z = m_fDistance * Math.sin(m_fXYPlaneRotation);
        var x = t * Math.cos(m_fZRotation);
        var y = t * Math.sin(m_fZRotation);

        m_oGLCamera.position.set(m_oOrigin.x + x, m_oOrigin.y + y, m_oOrigin.z + z);
        m_oGLCamera.lookAt(m_oOrigin);

        m_fMousePanSensitivity = m_fDistance * 0.0025;
    };
           
    PerspectiveCamera.prototype.Resize = function()
    {
        m_oGLCamera.aspect = window.innerWidth / window.innerHeight;
        m_oGLCamera.updateProjectionMatrix();
    };

    //#region Properties

    PerspectiveCamera.prototype.getGLCamera = function() {
        return m_oGLCamera;
    };

    PerspectiveCamera.prototype.getOrigin = function () {
        return m_oOrigin;
    };

    PerspectiveCamera.prototype.getDistance = function ()
    {
        return m_fDistance;
    };

    PerspectiveCamera.prototype.setStaticCamera = function (value)
    {
        m_bStaticCamera = value;
    };

    //#endregion
}


