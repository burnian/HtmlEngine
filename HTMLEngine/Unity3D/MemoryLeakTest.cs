using UnityEngine;
using HTMLEngine.Unity3D;
using System.Collections.Generic;
using UnityEngine.U2D;


public class MemoryLeakTest : MonoBehaviour
{
    private string testHtml = @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font>&nbsp;for&nbsp;<font color=lime>Unity3D.GUI</font>&nbsp;and&nbsp;<font color=lime>NGUI</font></font></p>
<br>
<br><p align=center valign=top>Picture <img inAtlas='false' src='smiles/sad'> with &lt;p valign=top&gt;</p>
<br><p align=center valign=middle>Picture <img src='atlases/atlases#smile'> with &lt;p valign=middle&gt; much better than others in this case <img src='atlases/atlases#cool'></p>
<br><p align=center valign=bottom>Picture <img src='atlases/atlases#sad'> with &lt;p valign=bottom&gt;</p>
<br><p align=center valign=bottom>Picture <anim src='atlases/atlases/power' fps=30 id='anim'> with &lt;img fps=10&gt;</p><br><p align=justify valign=bottom><img src='atlases/atlases#unity'> is a feature rich, fully integrated development engine for the creation of interactive 3D content. It provides complete, out-of-the-box functionality to assemble high-quality, high-performing content and publish to multiple platforms.</p>
<br><p align=center valign=bottom>Picture <anim src='atlases/atlases/power' fps=30 id='anim'> with &lt;img fps=10&gt;</p><br><p align=center><img src='atlases/atlases#unity2'></p>";


    private float pos = 0;
    private float m_deltaTime = 0f;
    private int flag = 1;

    private void Start()
    {
        //var tex = Resources.Load("atlases/smiles", typeof(Texture2D)) as Texture2D;
        //var atlas = Resources.Load("atlases/smiles", typeof(UIAtlas)) as UIAtlas;
        //var material = Resources.Load("atlases/smiles", typeof(Material)) as Material;
    }

    private void Update()
    {
        //m_deltaTime += Time.deltaTime;
        //if (m_deltaTime > 0.5f)
        //{
        //    m_deltaTime = 0f;
        //    Exec();
        //}
        Exec();
    }


    private void Exec()
    {
        var parent = GameObject.Find("icanvas").transform;
        if (flag > 0)
        {
            var go = new GameObject();
            go.transform.SetParent(parent);
            var u3dHtml = go.AddComponent<Unity3DHTML>();
            u3dHtml.maxLineWidth = 500;
            u3dHtml.html = testHtml;
            go.transform.position = new Vector3(pos, 550f, 0f);
            pos += 5;
        }
        else
        {
            DestroyChildren(parent);
        }
        flag *= -1;
    }

    private void DestroyChildren(Transform t)
    {
        bool isPlaying = Application.isPlaying;

        while (t.childCount != 0)
        {
            Transform child = t.GetChild(0);

            if (isPlaying)
            {
                child.parent = null;
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

}