using UnityEngine;
using System.Collections;
using SevenZip;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Threading;
using UnityEngine.UI;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {

	[SerializeField]
	Slider m_Slider;
	
	CodeProgress m_CodeProgress=null;//进度;

	Thread m_PackUPKThread=null;//UPK打包线程;
	Thread m_CompressThread=null;	//压缩线程;
	Thread m_DecompressThread=null;	//解压缩线程;
	Thread m_ExtraUPKThread=null;//释放UPK文件线程;

	string ApplicationdataPath=string.Empty;

	private float m_Percent=0f;
	
	void Start () 
	{
		ApplicationdataPath=Application.dataPath;

		m_CodeProgress=new CodeProgress(SetProgressPercent);
	}

	void Update()
	{
		if(m_Percent>0f)
		{
			m_Slider.value=m_Percent;
			if(m_Percent==1f)
			{
				Debug.Log("准备就绪");
				m_Percent=0f;
				#if UNITY_EDITOR
				AssetDatabase.Refresh();
				#endif
			}
		}
	}


	void OnGUI()
	{
		GUI.TextArea(
			new Rect(100,100,800,200),

			"文件夹压缩解压程序 \n\n" +
			"原理：本程序使用7Z压缩算法制作 http://www.7-zip.org/ 过程是文件夹⇔.UPK文件⇔.7Z文件 \n"+
			"注意事项：\n" +
			"1.请把要压缩的文件放在程序 Data 目录下 PackageFolder 文件夹中 \n" +
			"2.请执行 1- 2- 3- 4- 步骤，执行完毕以后请打开文件夹查看程序 Data 目录下的 OutPut 文件夹里面 文件是否能被正确解压出来\n" +
			"3.如果压缩和解压过程正常，则请保留程序自动生成的 Package_unitypackage.7z 文件 \n\n" +
			"谢谢使用 Design By 小小酥 syjsu@qq.com"
			);

		if(GUI.Button(new Rect(100,400,180,100),"1-文件夹打包成UPK文件"))
		{
			m_Slider.value=0f;
			m_PackUPKThread=new Thread(new ThreadStart(TestPackUPK));
			m_PackUPKThread.Start();
		}

		if(GUI.Button(new Rect(300,400,180,100),"2-压缩UPK文件成.7Z文件"))
		{
			m_Slider.value=0f;
			m_CompressThread=new Thread(new ThreadStart(TestCompress));
			m_CompressThread.Start();
		}

		if(GUI.Button(new Rect(500,400,180,100),"3-解压缩.7Z文件成UPK文件"))
		{
			m_Slider.value=0f;
			m_DecompressThread=new Thread(new ThreadStart(TestDeCompress));
			m_DecompressThread.Start();
		}

		if(GUI.Button(new Rect(700,400,180,100),"4-UPK文件还原成文件夹"))
		{
			m_Slider.value=0f;
			m_ExtraUPKThread=new Thread(new ThreadStart(TestExtraUPK));
			m_ExtraUPKThread.Start();
		}
	}

	void TestPackUPK()
	{
		try
		{
			if(!Directory.Exists(ApplicationdataPath+"/PackageFolder")){
				Directory.CreateDirectory(ApplicationdataPath+"/PackageFolder");
			}
			MyPackRes.PackFolder(ApplicationdataPath+"/PackageFolder",ApplicationdataPath+"/Package_compress.upk",m_CodeProgress);
		}
		catch(Exception ex)
		{
			Debug.Log(ex);
		}
	}

	void TestCompress()
	{
		try
		{
			LZMAHelper.Compress(ApplicationdataPath+"/Package_compress.upk",ApplicationdataPath+"/Package_unitypackage.7z",m_CodeProgress);
		}
		catch(Exception ex)
		{
			Debug.Log(ex);
		}
	}

	void TestDeCompress()
	{
		try
		{
			LZMAHelper.DeCompress(ApplicationdataPath+"/Package_unitypackage.7z",ApplicationdataPath+"/Package_decompress.upk",m_CodeProgress);
		}
		catch(Exception ex)
		{
			Debug.Log(ex);
		}
	}
	
	void TestExtraUPK()
	{
		try
		{
			if(!Directory.Exists(ApplicationdataPath+"/OutPut/")){
				Directory.CreateDirectory(ApplicationdataPath+"/OutPut/");
			}
			UPKExtra.ExtraUPK(ApplicationdataPath+"/Package_decompress.upk",ApplicationdataPath+"/OutPut/",m_CodeProgress);
		}
		catch(Exception ex)
		{
			Debug.Log(ex);
		}
	}

	void SetProgressPercent(Int64 fileSize,Int64 processSize)
	{
		m_Percent=(float)processSize/fileSize;
	}
}
