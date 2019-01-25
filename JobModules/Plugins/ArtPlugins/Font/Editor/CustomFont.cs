using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CustomFont : MonoBehaviour
{
    //��������ͨ�����е�sprite���������ļ�������ʹ�õ���unity�Դ���sprite editor�����������
    //���⣬����֮��ÿ��sprite�����ֵ����һ���ַ���Ӧ��ascii��ı��룬���磺
    //0�� ����ֻҪ��sprite������������xxx0���Ϳ����ˣ�
    //����ʹ�õ�����sprite���أ���������ͼƬ�����ResourcesĿ¼���棬��������ϣ��ٰ����Ƿŵ�fonts�ļ��л��������ļ����м��ɡ�
    [MenuItem ("Assets/CreateSpriteFont")]  
    static void CreateMyFontSprite ()
    {  

        Debug.LogWarning ("abc");  

        if (Selection.objects == null)
            return;  
        if (Selection.objects.Length == 0) {  
            Debug.LogWarning ("û��ѡ��Sprite�ļ�����Ҫ��Sprite Mode���ó�Multiple���зֺã������������ֵ����һ���ַ�����ascii��");  
            return;  
        }  
        UnityEngine.Object o = Selection.objects [0];  
        if (o.GetType () != typeof(Texture2D)) {  
            Debug.LogWarning ("ѡ�еĲ�����ͼƬ�ļ�");  
            return;  
        }  
        string selectionPath = AssetDatabase.GetAssetPath (o);  
        string selectionExt = Path.GetExtension (selectionPath);  
        if (selectionExt.Length == 0) {  
            return;  
        }  
        string loadPath = selectionPath.Remove (selectionPath.Length - selectionExt.Length);  
        string fontPathName = loadPath + ".fontsettings";  
        string matPathName = loadPath + ".mat";  
        float lineSpace = 0.1f;//�����м�࣬����������ߵ�����õ��м�࣬����ǹ̶��߶ȣ��������������е���  
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(selectionPath);
        List<Sprite> sprites = new List<Sprite>();
        for (var i = 0; i < objs.Length; i++)
        {
            if(objs[i].GetType() == typeof(Sprite)) sprites.Add(objs[i] as Sprite);
        }
        //        Sprite[] sprites = Resources.LoadAll<Sprite> (loadPath);  

        bool hasFont = true;
        if (sprites.Count > 0) {  
            //��textrue��ʽ��ø���Դ���������õ������Ĳ�����ȥ  
            Texture2D tex = o as Texture2D;  
            //����������ʣ����ҽ�ͼƬ���ú�  
            Material mat = new Material (Shader.Find ("GUI/Text Shader"));  
            
            mat.SetTexture ("_MainTex", tex);  
            //���������ļ������������ļ��Ĳ���  

            Font m_myFont = AssetDatabase.LoadAssetAtPath<Font>(fontPathName);
            if (m_myFont == null)
            {
                m_myFont = new Font ();
                hasFont = false;
            }  
            m_myFont.material = mat;
            
            //���������е��ַ�������  
            CharacterInfo[] characterInfo = new CharacterInfo[sprites.Count];   
            //�õ���ߵĸ߶ȣ������иߺͽ���ƫ�Ƽ���  
            for (int i = 0; i < sprites.Count; i++) {
                if ((sprites[i] as Sprite).rect.height > lineSpace)
                {
                    lineSpace = (sprites[i] as Sprite).rect.height;
                }
            }

            for (int i = 0; i < sprites.Count; i++) {
                Sprite spr = sprites[i] as Sprite;
                CharacterInfo info = new CharacterInfo();
                //����ascii�룬ʹ���з�sprite�����һ����ĸ  
                info.index = (int)spr.name[spr.name.Length - 1];
                Rect rect = spr.rect;
                //����pivot�����ַ���ƫ�ƣ�������Ҫ����ʲô���ģ����Ը����Լ���Ҫ�޸Ĺ�ʽ  
                float pivot = spr.pivot.y / rect.height - 0.5f;
                if (pivot > 0)
                {
                    pivot = -lineSpace / 2 - spr.pivot.y;
                }
                else if (pivot < 0)
                {
                    pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                }
                else
                {
                    pivot = -lineSpace / 2;
                }
                Debug.Log(pivot);
//                    int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                int offsetY = 0;
                //�����ַ�ӳ�䵽�����ϵ�����  
                info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                //�����ַ������ƫ��λ�úͿ��  
                info.minX = 0;
                info.minY = -(int)rect.height - offsetY;
                info.maxX = (int)rect.width;
                info.maxY = -offsetY;
                //�����ַ��Ŀ��  
                info.advance = (int)rect.width;
                characterInfo[i] = info;
            }
        // lineSpace += 2;  
            m_myFont.characterInfo = characterInfo;

            AssetDatabase.CreateAsset(mat, matPathName);
            if (!hasFont)
            {
                AssetDatabase.CreateAsset(m_myFont, fontPathName);
            }
            else
            {
                EditorUtility.SetDirty(m_myFont);//���ñ��������Դ  
                AssetDatabase.SaveAssets();//����������Դ  
            }
            
            
            AssetDatabase.Refresh ();//ˢ����Դ��ò����Mac�ϲ�������  

            var fontlbj = AssetDatabase.LoadAssetAtPath<Object>(fontPathName);
            SerializedObject so = new SerializedObject(fontlbj);
            Debug.Log(so.FindProperty("m_LineSpacing").floatValue);
            so.FindProperty("m_LineSpacing").floatValue = lineSpace;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(fontlbj);


            //��Ѹ߶ȣ����¸���һ�����صļ�࣬�������Ҫ����ע�͵��������������  
            //��ӡ��Ϊ��ʹʹ���߷�����д�иߣ���Ϊfont��֧�������иߡ�  
            Debug.Log ("��������ɹ�, ���߶ȣ�" + lineSpace + ", ��Ѹ߶ȣ�" + (lineSpace + 2));  
        } else {  
            Debug.LogWarning ("û��ѡ��Sprite�ļ�����Ҫ��Sprite�ŵ�Resources�ļ������棬���Բο������Ϸ���˵������");  
        }  
    }  
//    }
}