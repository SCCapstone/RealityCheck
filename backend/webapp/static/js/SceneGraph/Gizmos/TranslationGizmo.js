
function TranslationGizmo() 
{
    //#region Constructor

    var m_oPosition = new THREE.Vector3(0, -50, 5);

    var lineLength = 22;

    var m_oTranslationCone_X = new ModelEntity(new THREE.ConeGeometry(2.8, 8, 32), new THREE.MeshBasicMaterial({ color: 0xff0000 }));
    m_oTranslationCone_X.getGLMesh().position.set(-lineLength, 0, 0);
    m_oTranslationCone_X.getGLMesh().rotation.z = 90 * (Math.PI / 180);
    m_oTranslationCone_X.getGLMesh().name = "gizmoX";
    m_oTranslationCone_X.setName("Gizmo");

    var m_oTranslationCone_Y = new ModelEntity(new THREE.ConeGeometry(2.8, 8, 32), new THREE.MeshBasicMaterial({ color: 0x00ff00 }));
    m_oTranslationCone_Y.getGLMesh().position.set(0, -lineLength, 0);
    m_oTranslationCone_Y.getGLMesh().rotation.z = 180 * (Math.PI / 180);
    m_oTranslationCone_Y.getGLMesh().name = "gizmoY";
    m_oTranslationCone_Y.setName("Gizmo");

    var m_oTranslationCone_Z = new ModelEntity(new THREE.ConeGeometry(2.8, 8, 32), new THREE.MeshBasicMaterial({ color: 0x0000ff }));
    m_oTranslationCone_Z.getGLMesh().position.set(0, 0, lineLength);
    m_oTranslationCone_Z.getGLMesh().rotation.x = 90 * (Math.PI / 180);
    m_oTranslationCone_Z.getGLMesh().name = "gizmoZ";
    m_oTranslationCone_Z.setName("Gizmo");

    var oLineGeometry = new THREE.Geometry();

    oLineGeometry.vertices.push(
        new THREE.Vector3(0, 0, 0),
        new THREE.Vector3(-lineLength, 0, 0),

        new THREE.Vector3(0, 0, 0),
        new THREE.Vector3(0, -lineLength, 0),

        new THREE.Vector3(0, 0, 0),
        new THREE.Vector3(0, 0, lineLength)
    );

    //#endregion

    var m_oLines = new THREE.LineSegments(oLineGeometry, new THREE.MeshBasicMaterial({ color: 0xffffff }));

    TranslationGizmo.prototype.Reset = function()
    {
        m_oTranslationCone_X.setIsHighlighted(false);
        m_oTranslationCone_X.setIsSelected(false);
       
        m_oTranslationCone_Y.setIsHighlighted(false);
        m_oTranslationCone_Y.setIsSelected(false);
        
        m_oTranslationCone_Z.setIsHighlighted(false);
        m_oTranslationCone_Z.setIsSelected(false);
    };

    TranslationGizmo.prototype.Update = function(oCamera)
    {
        var gizmoScale = oCamera.getDistance() / 500;
        var oTempLineLength = 22 * gizmoScale;

        m_oTranslationCone_X.getGLMesh().position.set(m_oPosition.x - oTempLineLength,
                                                      m_oPosition.y,
                                                      m_oPosition.z);
        m_oTranslationCone_X.getGLMesh().scale.setScalar(gizmoScale);

        m_oTranslationCone_Y.getGLMesh().position.set(m_oPosition.x,
                                                      m_oPosition.y - oTempLineLength,
                                                      m_oPosition.z);
        m_oTranslationCone_Y.getGLMesh().scale.setScalar(gizmoScale);

        m_oTranslationCone_Z.getGLMesh().position.set(m_oPosition.x,
                                                      m_oPosition.y,
                                                      m_oPosition.z + oTempLineLength);
        m_oTranslationCone_Z.getGLMesh().scale.setScalar(gizmoScale);

        m_oLines.position.set(m_oPosition.x, m_oPosition.y, m_oPosition.z);
        m_oLines.scale.setScalar(gizmoScale);

        m_oTranslationCone_X.UpdateColors();
        m_oTranslationCone_Y.UpdateColors();
        m_oTranslationCone_Z.UpdateColors();
    };

    //#region Properties

    TranslationGizmo.prototype.setPosition = function(oVectorPosition)
    {
        m_oPosition = new THREE.Vector3(oVectorPosition.x, oVectorPosition.y, oVectorPosition.z);
    };

    TranslationGizmo.prototype.getXCone = function()
    {
        return m_oTranslationCone_X;
    };

    TranslationGizmo.prototype.getYCone = function ()
    {
        return m_oTranslationCone_Y;
    };

    TranslationGizmo.prototype.getZCone = function()
    {
        return m_oTranslationCone_Z;
    };

    TranslationGizmo.prototype.getLines = function ()
    {
        return m_oLines;
    };

    //#endregion
}