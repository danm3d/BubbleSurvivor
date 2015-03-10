using UnityEngine;
using System.Collections;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
public class SignIn : MonoBehaviour
{
	public void SignInUser()
	{
		if (!PlayGamesPlatform.Instance.localUser.authenticated)
		{
			PlayGamesPlatform.Instance.localUser.Authenticate((bool success) =>
			{
				if (success)
				{
					Debug.Log("We're signed in! Welcome " + PlayGamesPlatform.Instance.localUser.userName);
					// We could start our game now
				}
				else
				{
					Debug.Log("Oh... we're not signed in.");
				}
			});
		}
		else
		{
			Debug.Log("You're already signed in.");
			// We could also start our game now
		}
	}
}