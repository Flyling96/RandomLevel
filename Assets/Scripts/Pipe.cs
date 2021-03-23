using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public float radius = 0.5f;
    public byte slice = 20;
    public byte bendSlice = 10;
    public List<Vector3> listPoint;
    public Mesh m_Mesh = null;
    public Material mat;

    public List<Vector3> lstVertex = new List<Vector3>();
    public List<int> lstIndex = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        if (m_Mesh==null)
        {
            m_Mesh = new Mesh();
        }
        Build();
    }
    public void Build()
    {
        listPoint.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform node = transform.GetChild(i);
            listPoint.Add(node.localPosition);

        }
        if(listPoint.Count<2)
        {
            return;
        }
        if(m_Mesh!=null)
        {
            m_Mesh.Clear();
        }


        float bendOffset = radius * 2.0f;

        Vector3 up = Vector3.up;
        Vector3 last_forward = Vector3.forward;
        Vector3 last_right = Vector3.right;
        Vector3 last_end_p = Vector3.zero;
        Vector3 last_up = Vector3.up;
        for(int i=1;i<listPoint.Count;i++)
        {
            Vector3 lastp = listPoint[i - 1];
            Vector3 p = listPoint[i];

            Vector3 forward = (listPoint[i] - listPoint[i - 1]).normalized;

            if(Mathf.Abs(Vector3.Dot(forward,last_forward))<0.999f)
            {
                Quaternion q = Quaternion.FromToRotation(last_forward, forward);
                up = q * up;
                //right = q * right;
            }
            //if(i==1)
            //{
            //    if(Vector3.Dot(forwa))
            //}

            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;
           

            if (listPoint.Count == 2)
            {
                CircleVertex(lastp, right, up);
                AddPipePart(p, right, up);
            }
            else
            {
                
                if (i == 1)
                {
                    //开始的管道
                    CircleVertex(lastp, right, up);
                    AddPipePart(p - forward * bendOffset, right, up);
                }
                else if (i == listPoint.Count - 1)
                {
                    //结束管道
                    AddBendPipePart(last_end_p, lastp, lastp + forward * bendOffset, last_up, up, last_right, right);
                    AddPipePart( p, right, up);
                    
                }
                else
                {
                    //中间的管道
                    AddBendPipePart(last_end_p, lastp, lastp + forward * bendOffset, last_up, up, last_right, right);
                    AddPipePart(p - forward * bendOffset, right, up);
                    
                }
            }

            last_forward = forward;
            last_right = right;
            last_up = up;
            last_end_p = p - forward * bendOffset;
        }

        m_Mesh.vertices = lstVertex.ToArray();
        m_Mesh.triangles = lstIndex.ToArray();


    }

    void CircleVertex(Vector3 baseP, Vector3 right, Vector3 up)
    {
        for (int j = 0; j < slice; j++)
        {
            float angle = (float)j * 6.28318f / (float)slice;
            float fSin = Mathf.Sin(angle) * radius;
            float fCos = Mathf.Cos(angle) * radius;

            Vector3 temp = up * fSin + right * fCos;
            lstVertex.Add(baseP + temp);
        }
    }
    void AddCircleFace()
    {
        int firstVertex = lstVertex.Count-slice*2;

        int secondVertex = firstVertex + slice;
        for (int j = 0; j < slice; j++)
        {
            int v0 = firstVertex + j;
            int v1 = secondVertex + j;
            if (j == slice - 1)
            {

                lstIndex.Add(firstVertex);
                lstIndex.Add(v1);
                lstIndex.Add(v0);

                lstIndex.Add(secondVertex);
                lstIndex.Add(v1);
                lstIndex.Add(firstVertex);


            }
            else
            {
                lstIndex.Add(v0);
                
                lstIndex.Add(v0 + 1);
                lstIndex.Add(v1);

                lstIndex.Add(v1);
                lstIndex.Add(v0 + 1);

                lstIndex.Add(v1 + 1);
            }

            
        }
    }
    void AddPipePart(Vector3 end,Vector3 right,Vector3 up)
    {
        CircleVertex(end, right, up);
        AddCircleFace();
    }
    public void AddBendPipePart(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 up0,Vector3 up1,Vector3 right0,Vector3 right1)
    {
        for(int i=1;i<=bendSlice;i++)
        {
            float f = (float)i/(float)bendSlice;
            Vector3 temp0 = Vector3.Lerp(p0, p1, f);
            Vector3 temp1 = Vector3.Lerp(p1, p2, f);
            Vector3 tempPoint = Vector3.Lerp(temp0,temp1,f);

            Vector3 up = Vector3.Lerp(up0, up1, f).normalized;
            Vector3 right = Vector3.Lerp(right0, right1, f).normalized;

            CircleVertex(tempPoint, right, up);
            AddCircleFace();

        }
    }

    public void OnDestroy()
    {
        if(m_Mesh!=null)
        {
            Object.Destroy(m_Mesh);
            m_Mesh = null;
        }
    }

    //void AddSli

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMesh(m_Mesh, transform.localToWorldMatrix, mat, gameObject.layer);
        
    }
}
