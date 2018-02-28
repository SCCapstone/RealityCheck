
function BoundingBox(oMin, oMax) 
{
    //#region Constructor

    var m_oMin = oMin;
    var m_oMax = oMax;

    //#region Generate Box Mesh

    var oBoxGeometry = new THREE.Geometry();

    oBoxGeometry.vertices.push(
        //Lines going along X
        //================================================================================
        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMin.z),
        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMax.z),

        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMin.z),
        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMax.z),

        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMin.z),
        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMax.z),

        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMin.z),
        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMax.z),
        //================================================================================


        //Lines going along Y
        //================================================================================
        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMin.z),
        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMin.z),

        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMax.z),
        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMax.z),

        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMin.z),
        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMin.z),

        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMax.z),
        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMax.z),
        //================================================================================


        //Lines going along Z
        //================================================================================
        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMin.z),
        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMin.z),

        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMin.z),
        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMin.z),

        new THREE.Vector3(m_oMin.x, m_oMin.y, m_oMax.z),
        new THREE.Vector3(m_oMin.x, m_oMax.y, m_oMax.z),

        new THREE.Vector3(m_oMax.x, m_oMin.y, m_oMax.z),
        new THREE.Vector3(m_oMax.x, m_oMax.y, m_oMax.z)
        //================================================================================
    );

    this.m_oGLBoxMesh = new THREE.LineSegments(oBoxGeometry, new THREE.LineBasicMaterial({ color: 0xffff00 }));

    //#endregion

    //#endregion

    //#region Properties

    BoundingBox.prototype.getMin = function() {
        return m_oMin;
    };

    BoundingBox.prototype.getMax = function () {
        return m_oMax;
    };

    BoundingBox.prototype.getBoxMesh = function () {
        return this.m_oGLBoxMesh;
    };

    //#endregion

}