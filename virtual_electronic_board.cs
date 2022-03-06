using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

class VirtualElectronicBoard : Form
{
	private static Timer timer = new Timer();
	private Map mp = new Map(80, 32);
	private Label lb;
	private int dotSize=12;

	private string showText="【悲報】C＃できてもモテない。";
	private dotMapText dMT;
	private string[,] bitmap;
	private int posX=0, posY=8;

	public static void Main()
	{
		Application.Run(new VirtualElectronicBoard());
	}

	public VirtualElectronicBoard()
	{
		//ウィンドウタイトルの指定
		this.Text = "Virtual Electronic Board";
		//ウィンドウサイズの指定(px)
		this.Width = 1000; this.Height = 420;
		//ウィンドウの背景色をARGB値で指定(透明度, 赤, 緑, 青)
		this.BackColor = Color.FromArgb(255, 0,　0, 0);

		//タイマーの設定とON
		timer.Interval = 10;  //[ms]
		timer.Tick += new EventHandler(timer_Tick);
		timer.Enabled = true;

		//ラベルオブジェクト生成とフォント指定
		lb = new Label();
		lb.Font = new Font("MS UI Gothic", dotSize, GraphicsUnit.Pixel);
		//テキストの色指定　　cf. (255, 243, 152, 0); //オレンジ色でも結構良い
		lb.ForeColor = Color.FromArgb(255, 181, 255, 20); //緑色
		//ラベル配置位置とサイズ指定
		lb.Location = new Point(0,0);
		lb.Size = new Size(this.Width, this.Height);
		//ラベルのテキスト登録とフォーム内に配置
		lb.Text = mp.textMap();
		lb.Parent = this;

		//テキストのマップ情報を取得・bitmap(2次元string配列)に格納
		dMT = new dotMapText(showText);
		bitmap = dMT.getMap();

		//テキストをマップ内に表示
		displayText(posX, posY, showText, bitmap);
	}

	private void timer_Tick(Object sender, EventArgs e)
	{
		//テキスト表示位置を横にスクロール
		posX--;
		if(posX<-(showText.Length*16)) posX = mp.Width;

		displayText(posX, posY, showText, bitmap);
	}

	private void displayText(int x, int y, string text, string[,] bitmap)
	{
		int positionX, positionY;
		bool judgeDisplay=false;

		//ドットマップのリセット
		mp.Reset();

		//一文字ずつドット位置を指定してドット反転
		for(int i=0; i<text.Length; i++)
		{
			for(int j=0; j<dMT.fontSize; j++)
			{
				for(int k=0; k<bitmap[i,j].Length; k++)
				{
					positionX = x + k + dMT.fontSize * i;
					positionY = y + j;
					
					judgeDisplay = positionX>=0 && positionY>=0
									&& positionX<mp.Width && positionY<mp.Height;

					if(bitmap[i,j][k] == '1' && judgeDisplay)
					{
						mp.dotReverse(positionX, positionY);
					}
				}
			}
		}

		//ラベル内テキストの更新と再描画
		lb.Text = mp.textMap();
		lb.Parent = this;
	}
}

class Map
{
	//電光掲示板っぽいのは●―の気がする。。。
	//private string strT="■", strF="□", strN="\n";
	private string strT="●", strF="―", strN="\n";

	public string text;
	public string mapString;
	public StringBuilder sbmap;
	public int Width, Height;
	public int maxLength;

	public Map(int m, int n)
	{
		Width = m;
		Height = n;
		maxLength = m * n - 1;

		text = "";
		for(int i=0; i<n; i++)
		{
			for(int j=0; j<m; j++)
			{
				text += strF;
			}
			text += strN;
		}

		mapString = text;
		sbmap = new StringBuilder(mapString);
	}

	public void dotReverse(int m, int n)
	{
		if(m>Width-1 || n>Height-1) return;
		int changePoint = (Width+1) * n + m;

		if(sbmap.ToString()[changePoint] == strF[0])
		{
			sbmap.Insert(changePoint, strT, 1);
		}
		else
		{
			sbmap.Insert(changePoint, strF, 1);	
		}
		sbmap.Remove(changePoint+1, 1);
	}

	public int dotStatus(int m, int n)
	{
		if(m>Width-1 || n>Height-1) return 0;
		int getPoint = (Width+1) * n + m;

		if(sbmap.ToString()[getPoint] == strT[0]) return 1;
		else return 0;
	}

	public void Reset()
	{
		text = "";
		for(int i=0; i<Height; i++)
		{
			for(int j=0; j<Width; j++)
			{
				text += strF;
			}
			text += strN;
		}

		mapString = text;
		sbmap = new StringBuilder(mapString);
	}

	public void randomInput()
	{
		Random r = new Random();
		int randomN;

		text = "";
		for(int i=0; i<Height; i++)
		{
			for(int j=0; j<Width; j++)
			{
				randomN = (int)r.Next(0,2);

				if(randomN == 0) text += strF;
				else text += strT;
			}
			text += strN;
		}

		mapString = text;
		sbmap = new StringBuilder(mapString);
	}

	public string textMap()
	{
		return sbmap.ToString();
	}
}

class dotMapText
{
	private string[] char_CODE;
	private StreamReader sr;

	public int fontSize=16;
	public int textLength;
	public string[,] bitmap;


	public dotMapText(string text)
	{
		textLength = text.Length;

		sr = new StreamReader(@"./shinonomeFont/shnmk"+fontSize.ToString()+"i.bdf");

		char_CODE = new string[text.Length];
		byte[] bytesData;
		bitmap = new string[text.Length, fontSize];

		for(int chrNum=0; chrNum<text.Length; chrNum++)
		{
			//Shift JISとして文字列に変換
			bytesData = Encoding.GetEncoding("iso-2022-jp").GetBytes(text[chrNum].ToString());
			
			if(bytesData.Length == 1)
			{
				//0~9, A~Z
				char_CODE[chrNum] += "23";
				char_CODE[chrNum] += string.Format("{0:x2}", bytesData[0]);
			}
			if(bytesData.Length == 8)
			{
				//2バイト文字の場合、3バイト目が区、4バイト目が点
				char_CODE[chrNum] += string.Format("{0:x2}", bytesData[3]);
				char_CODE[chrNum] += string.Format("{0:x2}", bytesData[4]);
			}
		}

		sr.Close();
	}

	public string[,] getMap()
	{
		string findCode;
		string line;

		sr = new StreamReader(@"./shinonomeFont/shnmk"+fontSize.ToString()+"i.bdf");

		for(int chrNum=0; chrNum<textLength; chrNum++)
		{
			findCode=char_CODE[chrNum];

			while((line = sr.ReadLine()) != null)
			{
				if(line.IndexOf("STARTCHAR " + findCode) >= 0)
				{
					for(int i=0; i<5; i++) sr.ReadLine();

					for(int i=0; i<fontSize; i++)
					{
						line = sr.ReadLine();
						bitmap[chrNum,i] = "";

						for(int j=0; j<line.Length; j++)
						{
							bitmap[chrNum,i] += Convert.ToString(Convert.ToInt32(line[j].ToString(), 16), 2).PadLeft(4, '0');
						}
						//Console.WriteLine(bitmap[chrNum,i]);
					}
					break;
				}
			}
			//読み込み一の初期化
			sr = new StreamReader(@"./shinonomeFont/shnmk16i.bdf");
		}

		sr.Close();
		return bitmap;
	}
}