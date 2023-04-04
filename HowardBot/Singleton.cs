namespace HowardBot
{
	public class Singleton<T> where T: new()
	{
		private static T m_instance;

		public static T Instance
		{
			get
			{
				if (m_instance == null)
					m_instance = new T();

				return m_instance;
			}
		}
	}
}