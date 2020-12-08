using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	/// <summary>This component allows you to modify a value passed from an event and emit it again.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanModifyFloat")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Modify Float")]
	public class LeanModifyFloat : MonoBehaviour
	{
		[System.Serializable] public class FloatEvent : UnityEvent<float> {}

		/// <summary>The value will be incremented by this.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		/// <summary>The value will be multiplied by this.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] private float multiplier;

		/// <summary>The modified value will be output from this event.</summary>
		public FloatEvent OnModified { get { if (onModified == null) onModified = new FloatEvent(); return onModified; } } [SerializeField] private FloatEvent onModified;

		public void ModifyValue(float value)
		{
			if (onModified != null)
			{
				value += offset;
				value *= multiplier;

				onModified.Invoke(value);
			}
		}
	}
}