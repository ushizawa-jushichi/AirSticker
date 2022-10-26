

using UnityEngine;

namespace CyDecal.Runtime.Scripts
{
    /// <summary>
    /// 凸多角形ポリゴン
    /// </summary>
    public class CyConvexPolygon 
    {
        public Vector3 FaceNormal { get; private set; }                 // 面法線
        private const int MaxVertex = 64;                               // 凸多角形の最大頂点
        private readonly Vector3[] _vertices = new Vector3[MaxVertex];  // 頂点座標
        private readonly Vector3[] _normals = new Vector3[MaxVertex];   // 頂点法線
        private readonly CyLine[] _line = new CyLine[MaxVertex];        // 凸多角形を構成するエッジの情報
        private int _numVertices;                                       // 頂点数
        public int NumVertices => _numVertices;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertices">多角形を構築する頂点の座標</param>
        /// <param name="normals">多角形を構築する頂点の法線</param>
        public CyConvexPolygon(Vector3[] vertices, Vector3[] normals)
        {
            _numVertices = vertices.Length;
            for (int vertNo = 0; vertNo < _numVertices; vertNo++)
            {
                this._vertices[vertNo] = vertices[vertNo];
                this._normals[vertNo] = normals[vertNo];
                int nextVertNo = (vertNo + 1) % _numVertices; 
                _line[vertNo] = new CyLine( 
                    vertices[vertNo], 
                    vertices[nextVertNo],
                    normals[vertNo],
                    normals[nextVertNo]);
            }
            FaceNormal = Vector3.Cross((vertices[1] - vertices[0]), (vertices[2] - vertices[0]));
            FaceNormal.Normalize();
        }
        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="srcConvecPolygon">コピー元となる凸多角形</param>
        public CyConvexPolygon(CyConvexPolygon srcConvecPolygon)
        {
            _numVertices = srcConvecPolygon._numVertices;
            for (int i = 0; i < _numVertices; i++)
            {
                this._vertices[i] = srcConvecPolygon._vertices[i];
                this._normals[i] = srcConvecPolygon._normals[i];
                this._line[i] = srcConvecPolygon._line[i];
            }
            FaceNormal = srcConvecPolygon.FaceNormal;
        }
        /// <summary>
        /// 頂点座標を取得。
        /// </summary>
        /// <param name="vertNo">頂点番号</param>
        /// <returns></returns>
        public Vector3 GetVertexPosition(int vertNo)
        {
            return _vertices[vertNo];
        }
        /// <summary>
        /// 頂点法線を取得
        /// </summary>
        /// <param name="vertNo">頂点番号</param>
        /// <returns></returns>
        public Vector3 GetVertexNormal(int vertNo)
        {
            return _normals[vertNo];
        }
        /// <summary>
        /// Vector3からVector4(wは1)に変換します。
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector4 Vector3ToVector4(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1.0f);
        }

        /// <summary>
        /// 凸多角形を平面で分割/削除する。
        /// </summary>
        /// <remarks>
        /// 平面により凸多角形を分割できる場合に分割処理を行い、新しい凸多角形を作成します。
        /// また、この際に平面の外側(負の側)にある頂点は破棄されます。
        /// 例えば、三角形を平面で分割する場合、四角形と三角形に分割されますが、
        /// この時、分割後のどちらかの多角形（平面の外側の多角形）の情報は失われます。
        /// また、凸多角形を構成する全ての頂点が平面の外側にあった場合はallVertexIsOutsideにtrueを設定します。
        /// </remarks>
        /// <param name="clipPlane">分割平面</param>
        /// <param name="allVertexIsOutside">凸多角形の全ての頂点が平面の外の場合にtrueが設定されます。</param>
        public void SplitAndRemoveByPlane(Vector4 clipPlane, out bool allVertexIsOutside)
        {
            allVertexIsOutside = false;
            // クリップ平面の外側にある頂点を調べる。
            int numOutsideVertex = 0;
            int removeVertStartNo = -1;
            int removeVertEndNo = 0;
            int remainVertStartNo = -1;
            int remainVertEndNo = 0;
            for (int no = 0; no < _numVertices; no++)
            {
                var t = Vector4.Dot(clipPlane, Vector3ToVector4(_vertices[no]));
                if (t < 0)
                {
                    // 外側
                    if (removeVertStartNo == -1)
                    {
                        removeVertStartNo = no;
                    }
                    removeVertEndNo = no;
                    numOutsideVertex++;
                }
                else
                {
                    // 内側
                    if (remainVertStartNo == -1)
                    {
                        remainVertStartNo = no;
                    }

                    remainVertEndNo = no;
                }
            }
            if (numOutsideVertex == _numVertices)
            {
                // 全ての頂点がクリップ平面の外側にいるので分割は行えない。
                allVertexIsOutside = true;
                return;
            }

            if (numOutsideVertex == 0)
            {
                // 全ての頂点が内側なので分割は行えない。
                return;
            }
            // ここから多角形分割。
            // 多角形の辺と平面が交差する箇所に新しい頂点が二つ増える。
            // また、平面の外側の頂点は除外するので、多角形の頂点の増減値は 2 - numOutsideVertex となる。 
            int deltaVerticesSize = 2 - numOutsideVertex ;
            
            if (removeVertStartNo == 0)
            {
                // 0番目の頂点が除外される
                // 平面と交差する二つのラインの情報をバックアップ。
                CyLine l0 = _line[remainVertStartNo - 1];
                CyLine l1 = _line[remainVertEndNo];
                // 残る頂点を前方に詰める。
                int vertNo = 0;
                for (int i = remainVertStartNo; i < remainVertEndNo + 1; i++)
                {
                    _vertices[vertNo] = _vertices[i];
                    _normals[vertNo] = _normals[i];
                    _line[vertNo] = _line[i];
                    vertNo++;
                }
                // 頂点を二つ追加する。
                int newVertNo_0 = vertNo;
                int newVertNo_1 = vertNo+1;
                
                float t = Vector4.Dot(clipPlane, Vector3ToVector4(l1.startPosition))
                    /Vector4.Dot(clipPlane,l1.startPosition - l1.endPosition);
                
                var newVert0 = Vector3.Lerp(l1.startPosition, l1.endPosition, t);
                var newNormal0 = Vector3.Lerp(l1.startNormal, l1.endNormal, t);
                newNormal0.Normalize();
                
                t = Vector4.Dot(clipPlane, Vector3ToVector4(l0.endPosition))
                          /Vector4.Dot(clipPlane,l0.startToEndVec);
                var newVert1 = Vector3.Lerp(l0.endPosition, l0.startPosition,  t);
                var newNormal1 = Vector3.Lerp(l0.endNormal, l0.startNormal,  t);
                newNormal1.Normalize();
                
                _vertices[newVertNo_0] = newVert0;
                _vertices[newVertNo_1] = newVert1;
                _normals[newVertNo_0] = newNormal0;
                _normals[newVertNo_1] = newNormal1;
                
                // ライン情報の構築。
                _numVertices += deltaVerticesSize;
                _line[newVertNo_0-1].SetEndAndCalcStartToEnd(newVert0, newNormal0);
                _line[newVertNo_0].SetStartEndAndCalcStartToEnd(
                    newVert0, 
                    newVert1,
                    newNormal0,
                    newNormal1);
                _line[newVertNo_1].SetStartEndAndCalcStartToEnd(
                    newVert1,
                    _vertices[(newVertNo_1+1)%_numVertices],
                    newNormal1,
                    _normals[(newVertNo_1+1)%_numVertices]);
                
                _line[newVertNo_1+1].SetStartAndCalcStartToEnd(
                    _vertices[(newVertNo_1+1)%_numVertices],
                    _normals[(newVertNo_1+1)%_numVertices]);
                
            }
            else
            {
                // それ以外
                // 平面と交差する二つのラインの情報をバックアップ。
                CyLine l0 = _line[removeVertStartNo - 1];
                CyLine l1 = _line[removeVertEndNo];
                if (deltaVerticesSize > 0)
                {
                    // 頂点が増える。
                    for (int i = _numVertices - 1; i > removeVertEndNo; i--)
                    {
                        _vertices[i + deltaVerticesSize] = _vertices[i];
                        _normals[i + deltaVerticesSize] = _normals[i];
                        _line[i + deltaVerticesSize] = _line[i];
                    }
                }
                else
                {
                    // 頂点が減る or 同じ
                    for (int i = removeVertEndNo + 1; i < _numVertices; i++)
                    {
                        _vertices[i + deltaVerticesSize] = _vertices[i];
                        _normals[i + deltaVerticesSize] = _normals[i];
                        _line[i + deltaVerticesSize] = _line[i];
                    }
                }
                // 頂点を二つ追加する。
                int newVertNo_0 = removeVertStartNo;
                int newVertNo_1 = removeVertStartNo+1;
                float t = Vector4.Dot(clipPlane, Vector3ToVector4(l0.endPosition))
                          /Vector4.Dot(clipPlane,l0.startToEndVec);
                var newVert0 = Vector3.Lerp(l0.endPosition, l0.startPosition, t);
                var newNormal0 = Vector3.Lerp(l0.endNormal, l0.startNormal, t);
                newNormal0.Normalize();
                
                t = Vector4.Dot(clipPlane, Vector3ToVector4(l1.startPosition))
                    /Vector4.Dot(clipPlane,l1.startPosition - l1.endPosition);
                
                var newVert1 = Vector3.Lerp(l1.startPosition, l1.endPosition,t);
                var newNormal1 = Vector3.Lerp(l1.startNormal, l1.endNormal,t);
                newNormal1.Normalize();
                
                _vertices[newVertNo_0] = newVert0;
                _vertices[newVertNo_1] = newVert1;
                _normals[newVertNo_0] = newNormal0;
                _normals[newVertNo_1] = newNormal1;
                
                // ライン情報の構築。
                _numVertices += deltaVerticesSize;
                _line[newVertNo_0-1].SetEndAndCalcStartToEnd(newVert0, newNormal0);
                _line[newVertNo_0].SetStartEndAndCalcStartToEnd(
                    newVert0, 
                    newVert1,
                    newNormal0,
                    newNormal1);
                _line[newVertNo_1].SetStartEndAndCalcStartToEnd(
                    newVert1,
                    _vertices[(newVertNo_1+1)%_numVertices],
                    newNormal1,
                    _normals[(newVertNo_1+1)%_numVertices]);
                _line[newVertNo_1+1].SetStartAndCalcStartToEnd(
                    _vertices[(newVertNo_1+1)%_numVertices],
                    _normals[(newVertNo_1+1)%_numVertices]);
            }
        }

        /// <summary>
        /// レイと三角形の衝突判定。
        /// </summary>
        /// <remarks>
        /// 凸多角形が三角形意外の時はfalseを返します。
        /// </remarks>
        /// <param name="hitPoint">衝突している場合は衝突点の座標が記憶されます。</param>
        /// <param name="rayStartPos">レイの始点の座標</param>
        /// <param name="rayEndPos">レイの終点の座標</param>
        /// <returns>衝突している場合はtrueを返します。</returns>
        public bool IsIntersectRayToTriangle(ref Vector3 hitPoint, Vector3 rayStartPos, Vector3 rayEndPos)
        {
            if (_numVertices != 3)
            {
                return false;
            }
            var v0_pos = _vertices[0];
            var v1_pos = _vertices[1];
            var v2_pos = _vertices[2];

            // 平面とレイの交差を調べる。
            var v0_to_rayStart = rayStartPos - v0_pos;
            var v0_to_rayEnd = rayEndPos - v0_pos;
            var v0_to_rayStartNorm = v0_to_rayStart.normalized;
            var v0_to_rayEndNorm = v0_to_rayEnd.normalized;
            var t = Vector3.Dot(v0_to_rayStartNorm, FaceNormal) 
                    * Vector3.Dot(v0_to_rayEndNorm, FaceNormal);
            if (t < 0.0f)
            {
                // 交差している。
                // 次は交点を計算。
                var t0 = Mathf.Abs(Vector3.Dot(v0_to_rayStart, FaceNormal));
                var t1 = Mathf.Abs(Vector3.Dot(v0_to_rayEnd, FaceNormal));
                var intersectPoint = Vector3.Lerp(rayStartPos, rayEndPos, t0 / (t0 + t1));
                // 続いて、交点が三角形の中かどうかを調べる。
                var v0_to_intersectPos = intersectPoint - v0_pos;
                var v1_to_intersectPos = intersectPoint - v1_pos;
                var v2_to_intersectPos = intersectPoint - v2_pos;
                v0_to_intersectPos.Normalize();
                v1_to_intersectPos.Normalize();
                v2_to_intersectPos.Normalize();
                var v0_to_v1 = _line[0].startToEndVec;
                var v1_to_v2 = _line[1].startToEndVec;
                var v2_to_v0 = _line[2].startToEndVec;
                v0_to_v1.Normalize();
                v1_to_v2.Normalize();
                v2_to_v0.Normalize();

                var a0 = Vector3.Cross(v0_to_v1,v0_to_intersectPos );
                var a1 = Vector3.Cross(v1_to_v2,v1_to_intersectPos );
                var a2 = Vector3.Cross(v2_to_v0,v2_to_intersectPos );
                a0.Normalize();
                a1.Normalize();
                a2.Normalize();
            
                if (Vector3.Dot(a0, a1) > 0.0f 
                    && Vector3.Dot(a0, a2) > 0.0f)
                {
                    // 三角形の中だったので、交差していることが確定。
                    hitPoint = intersectPoint;
                    return true;
                }
            }

            return false;
        }
    }
    
}