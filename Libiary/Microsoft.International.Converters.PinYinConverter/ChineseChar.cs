using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Microsoft.International.Converters.PinYinConverter
{
    /// <summary>
    /// ��װ�˼������ĵĶ����ͱʻ��Ȼ�����Ϣ��
    /// </summary>
	public class ChineseChar
	{
		private const short MaxPolyphoneNum = 8;

		private static CharDictionary charDictionary;

		private static PinyinDictionary pinyinDictionary;

		private static StrokeDictionary strokeDictionary;

		private static HomophoneDictionary homophoneDictionary;

		private char chineseCharacter;

		private short strokeNumber;

		private bool isPolyphone;

		private short pinyinCount;

		private string[] pinyinList = new string[8];
        /// <summary>
        /// ��ȡ����ַ���ƴ��������
        /// </summary>
		public short PinyinCount
		{
			get
			{
				return this.pinyinCount;
			}
		}
        /// <summary>
        /// ��ȡ����ַ��ıʻ�����
        /// </summary>
		public short StrokeNumber
		{
			get
			{
				return this.strokeNumber;
			}
		}
        /// <summary>
        /// ��ȡ����ַ��Ƿ��Ƕ����֡�
        /// </summary>
		public bool IsPolyphone
		{
			get
			{
				return this.isPolyphone;
			}
		}
        /// <summary>
        /// ��ȡ����ַ���ƴ�����ü��ϳ��Ȳ��ܱ�ʾʵ��ƴ��������ʵ��ƴ��������ʹ��PinyinCount�ֶ�
        /// </summary>
		public ReadOnlyCollection<string> Pinyins
		{
			get
			{
				return new ReadOnlyCollection<string>(this.pinyinList);
			}
		}
        /// <summary>
        /// ��ȡ��������ַ���
        /// </summary>
		public char ChineseCharacter
		{
			get
			{
				return this.chineseCharacter;
			}
		}

		static ChineseChar()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream("Microsoft.International.Converters.PinYinConverter.PinyinDictionary.resources"))
			{
				using (ResourceReader resourceReader = new ResourceReader(manifestResourceStream))
				{
					string text;
					byte[] buffer;
					resourceReader.GetResourceData("PinyinDictionary", out text, out buffer);
					using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer)))
					{
						ChineseChar.pinyinDictionary = PinyinDictionary.Deserialize(binaryReader);
					}
				}
			}
			using (Stream manifestResourceStream2 = executingAssembly.GetManifestResourceStream("Microsoft.International.Converters.PinYinConverter.CharDictionary.resources"))
			{
				using (ResourceReader resourceReader2 = new ResourceReader(manifestResourceStream2))
				{
					string text;
					byte[] buffer;
					resourceReader2.GetResourceData("CharDictionary", out text, out buffer);
					using (BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(buffer)))
					{
						ChineseChar.charDictionary = CharDictionary.Deserialize(binaryReader2);
					}
				}
			}
			using (Stream manifestResourceStream3 = executingAssembly.GetManifestResourceStream("Microsoft.International.Converters.PinYinConverter.HomophoneDictionary.resources"))
			{
				using (ResourceReader resourceReader3 = new ResourceReader(manifestResourceStream3))
				{
					string text;
					byte[] buffer;
					resourceReader3.GetResourceData("HomophoneDictionary", out text, out buffer);
					using (BinaryReader binaryReader3 = new BinaryReader(new MemoryStream(buffer)))
					{
						ChineseChar.homophoneDictionary = HomophoneDictionary.Deserialize(binaryReader3);
					}
				}
			}
			using (Stream manifestResourceStream4 = executingAssembly.GetManifestResourceStream("Microsoft.International.Converters.PinYinConverter.StrokeDictionary.resources"))
			{
				using (ResourceReader resourceReader4 = new ResourceReader(manifestResourceStream4))
				{
					string text;
					byte[] buffer;
					resourceReader4.GetResourceData("StrokeDictionary", out text, out buffer);
					using (BinaryReader binaryReader4 = new BinaryReader(new MemoryStream(buffer)))
					{
						ChineseChar.strokeDictionary = StrokeDictionary.Deserialize(binaryReader4);
					}
				}
			}
		}
        /// <summary>
        /// ChineseChar��Ĺ��캯����
        /// </summary>
        /// <param name="ch">ָ���ĺ����ַ���</param>
		public ChineseChar(char ch)
		{
			if (!ChineseChar.IsValidChar(ch))
			{
				throw new NotSupportedException(AssemblyResource.CHARACTER_NOT_SUPPORTED);
			}
			this.chineseCharacter = ch;
			CharUnit charUnit = ChineseChar.charDictionary.GetCharUnit(ch);
			this.strokeNumber = (short)charUnit.StrokeNumber;
			this.pinyinCount = (short)charUnit.PinyinCount;
			this.isPolyphone = (charUnit.PinyinCount > 1);
			for (int i = 0; i < (int)this.pinyinCount; i++)
			{
				PinyinUnit pinYinUnitByIndex = ChineseChar.pinyinDictionary.GetPinYinUnitByIndex((int)charUnit.PinyinIndexList[i]);
				this.pinyinList[i] = pinYinUnitByIndex.Pinyin;
			}
		}
        /// <summary>
        /// ʶ���ַ��Ƿ���ָ���Ķ�����
        /// </summary>
        /// <param name="pinyin">ָ������Ҫ��ʶ���ƴ��</param>
        /// <returns>���ָ����ƴ���ַ�����ʵ���ַ���ƴ���������򷵻�ture�����򷵻�false�� </returns>
		public bool HasSound(string pinyin)
		{
			if (pinyin == null)
			{
				throw new ArgumentNullException("HasSound_pinyin");
			}
			for (int i = 0; i < (int)this.PinyinCount; i++)
			{
				if (string.Compare(this.Pinyins[i], pinyin, true, CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}
        /// <summary>
        /// ʶ��������ַ��Ƿ���ʵ���ַ���ͬ���֡� 
        /// </summary>
        /// <param name="ch">ָ����Ҫʶ����ַ���</param>
        /// <returns>���������ʵ���ַ���ͬ�����򷵻�ture�����򷵻�false��</returns>
		public bool IsHomophone(char ch)
		{
			return ChineseChar.IsHomophone(this.chineseCharacter, ch);
        }
        /// <summary>
        /// ʶ������������ַ��Ƿ���ͬ���֡�
        /// </summary>
        /// <param name="ch1">ָ����һ���ַ�</param>
        /// <param name="ch2">ָ���ڶ����ַ�</param>
        /// <returns>����������ַ���ͬ���ַ���ture�����򷵻�false��</returns>
		public static bool IsHomophone(char ch1, char ch2)
		{
			CharUnit charUnit = ChineseChar.charDictionary.GetCharUnit(ch1);
			CharUnit charUnit2 = ChineseChar.charDictionary.GetCharUnit(ch2);
			return ChineseChar.ExistSameElement<short>(charUnit.PinyinIndexList, charUnit2.PinyinIndexList);
        }
        /// <summary>
        /// ���������ַ���ʵ���ַ��ıʻ������бȽϡ�
        /// </summary>
        /// <param name="ch">��ʾ�������ַ�</param>
        /// <returns>˵���Ƚϲ����Ľ���� ��������ַ���ʵ���ַ��ıʻ�����ͬ������ֵΪ 0�� ���ʵ���ַ��ȸ����ַ��ıʻ��࣬����ֵΪ> 0�� ���ʵ���ַ��ȸ����ַ��ıʻ��٣�����ֵΪ &lt; 0�� </returns>
		public int CompareStrokeNumber(char ch)
		{
			CharUnit charUnit = ChineseChar.charDictionary.GetCharUnit(ch);
			return (int)(this.StrokeNumber - (short)charUnit.StrokeNumber);
        }
        /// <summary>
        /// ��ȡ����ƴ��������ͬ���֡�
        /// </summary>
        /// <param name="pinyin">ָ��������</param>
        /// <returns>���ؾ�����ͬ��ָ��ƴ�����ַ����б� ���ƴ��������Чֵ�򷵻ؿա� </returns>
		public static char[] GetChars(string pinyin)
		{
			if (pinyin == null)
			{
				throw new ArgumentNullException("pinyin");
			}
			if (!ChineseChar.IsValidPinyin(pinyin))
			{
				return null;
			}
			HomophoneUnit homophoneUnit = ChineseChar.homophoneDictionary.GetHomophoneUnit(ChineseChar.pinyinDictionary, pinyin);
			return homophoneUnit.HomophoneList;
        }
        /// <summary>
        /// ʶ�������ƴ���Ƿ���һ����Ч��ƴ���ַ�����
        /// </summary>
        /// <param name="pinyin">ָ����Ҫʶ����ַ�����</param>
        /// <returns>���ָ�����ַ�����һ����Ч��ƴ���ַ����򷵻�ture�����򷵻�false��</returns>
		public static bool IsValidPinyin(string pinyin)
		{
			if (pinyin == null)
			{
				throw new ArgumentNullException("pinyin");
			}
			return ChineseChar.pinyinDictionary.GetPinYinUnitIndex(pinyin) >= 0;
        }
        /// <summary>
        /// ʶ��������ַ����Ƿ���һ����Ч�ĺ����ַ���
        /// </summary>
        /// <param name="ch">ָ����Ҫʶ����ַ���</param>
        /// <returns>���ָ�����ַ���һ����Ч�ĺ����ַ��򷵻�ture�����򷵻�false��</returns>
		public static bool IsValidChar(char ch)
		{
			CharUnit charUnit = ChineseChar.charDictionary.GetCharUnit(ch);
			return charUnit != null;
		}
        /// <summary>
        /// ʶ������ıʻ����Ƿ���һ����Ч�ıʻ�����
        /// </summary>
        /// <param name="strokeNumber">ָ����Ҫʶ��ıʻ�����</param>
        /// <returns>���ָ���ıʻ�����һ����Ч�ıʻ����򷵻�ture�����򷵻�false��</returns>
		public static bool IsValidStrokeNumber(short strokeNumber)
		{
			if (strokeNumber < 0 || strokeNumber > 48)
			{
				return false;
			}
			StrokeUnit strokeUnit = ChineseChar.strokeDictionary.GetStrokeUnit((int)strokeNumber);
			return strokeUnit != null;
        }
        /// <summary>
        /// ��������ָ��ƴ�����ַ�����
        /// </summary>
        /// <param name="pinyin">��ʾ��Ҫ��ʶ���ƴ���ַ�����</param>
        /// <returns>���ؾ���ָ��ƴ�����ַ����� ���ƴ��������Чֵ�򷵻�-1��</returns>
		public static short GetHomophoneCount(string pinyin)
		{
			if (pinyin == null)
			{
				throw new ArgumentNullException("pinyin");
			}
			if (!ChineseChar.IsValidPinyin(pinyin))
			{
				return -1;
			}
			return ChineseChar.homophoneDictionary.GetHomophoneUnit(ChineseChar.pinyinDictionary, pinyin).Count;
        }
        /// <summary>
        /// ����ָ���ַ��ıʻ����� 
        /// </summary>
        /// <param name="ch">ָ����Ҫʶ����ַ���</param>
        /// <returns>����ָ���ַ��ıʻ����� ����ַ�������Чֵ�򷵻�-1�� </returns>
		public static short GetStrokeNumber(char ch)
		{
			if (!ChineseChar.IsValidChar(ch))
			{
				return -1;
			}
			CharUnit charUnit = ChineseChar.charDictionary.GetCharUnit(ch);
			return (short)charUnit.StrokeNumber;
        }
        /// <summary>
        /// ��������ָ���ʻ����������ַ�����
        /// </summary>
        /// <param name="strokeNumber">ָ����Ҫ��ʶ��ıʻ�����</param>
        /// <returns>���ؾ���ָ���ʻ������ַ��б� ����ʻ�������Чֵ���ؿա�</returns>
		public static char[] GetChars(short strokeNumber)
		{
			if (!ChineseChar.IsValidStrokeNumber(strokeNumber))
			{
				return null;
			}
			StrokeUnit strokeUnit = ChineseChar.strokeDictionary.GetStrokeUnit((int)strokeNumber);
			return strokeUnit.CharList;
        }
        /// <summary>
        /// ��������ָ���ʻ������ַ�������
        /// </summary>
        /// <param name="strokeNumber">��ʾ��Ҫ��ʶ��ıʻ�����</param>
        /// <returns>���ؾ���ָ���ʻ������ַ����� ����ʻ�������Чֵ����-1��</returns>
		public static short GetCharCount(short strokeNumber)
		{
			if (!ChineseChar.IsValidStrokeNumber(strokeNumber))
			{
				return -1;
			}
			return ChineseChar.strokeDictionary.GetStrokeUnit((int)strokeNumber).CharCount;
		}

		private static bool ExistSameElement<T>(T[] array1, T[] array2) where T : IComparable
		{
			int num = 0;
			int num2 = 0;
			while (num < array1.Length && num2 < array2.Length)
			{
				if (array1[num].CompareTo(array2[num2]) < 0)
				{
					num++;
				}
				else
				{
					if (array1[num].CompareTo(array2[num2]) <= 0)
					{
						return true;
					}
					num2++;
				}
			}
			return false;
		}
	}
}
