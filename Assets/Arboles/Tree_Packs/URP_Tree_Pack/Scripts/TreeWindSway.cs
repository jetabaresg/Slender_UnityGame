using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TreeWindSway : MonoBehaviour
{
	[Header("Targets")]
	[SerializeField] private bool affectChildren = true;
	[SerializeField] private List<Transform> extraLeafTargets = new List<Transform>();

	[Header("Wind")]
	[SerializeField, Min(0f)] private float swayAngle = 4f;
	[SerializeField, Min(0f)] private float swaySpeed = 1.5f;
	[SerializeField, Min(0f)] private float turbulence = 0.8f;
	[SerializeField] private Vector3 swayAxis = new Vector3(0f, 0f, 1f);
	[SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0f);
	[SerializeField, Min(0f)] private float spatialVariation = 0.2f;
	[SerializeField] private bool useUnscaledTime;

	private readonly List<LeafTarget> targets = new List<LeafTarget>();

	private sealed class LeafTarget
	{
		public Transform Transform;
		public Quaternion BaseRotation;
		public float Phase;
	}

	private void OnEnable()
	{
		CacheTargets();
	}

	private void OnDisable()
	{
		RestoreBaseRotations();
	}

	private void OnValidate()
	{
		if (swayAxis.sqrMagnitude < 0.0001f)
		{
			swayAxis = new Vector3(0f, 0f, 1f);
		}

		if (windDirection.sqrMagnitude < 0.0001f)
		{
			windDirection = new Vector3(1f, 0f, 0f);
		}
	}

	private void Update()
	{
		if (targets.Count == 0)
		{
			return;
		}

		float time = useUnscaledTime ? Time.unscaledTime : Time.time;
		Vector3 axis = swayAxis.normalized;
		Vector3 wind = windDirection.normalized;

		for (int index = 0; index < targets.Count; index++)
		{
			LeafTarget target = targets[index];

			if (target.Transform == null)
			{
				continue;
			}

			float positionWave = Vector3.Dot(target.Transform.position, wind) * spatialVariation;
			float baseWave = Mathf.Sin((time * swaySpeed) + target.Phase + positionWave);
			float detailWave = Mathf.Sin((time * swaySpeed * 2.13f) + (target.Phase * 1.37f));
			float angle = swayAngle * (baseWave + (detailWave * turbulence * 0.35f));

			target.Transform.localRotation = target.BaseRotation * Quaternion.AngleAxis(angle, axis);
		}
	}

	[ContextMenu("Recache Targets")]
	private void CacheTargets()
	{
		targets.Clear();
		HashSet<Transform> uniqueTargets = new HashSet<Transform>();

		if (affectChildren)
		{
			Transform[] childTransforms = GetComponentsInChildren<Transform>(true);
			for (int index = 0; index < childTransforms.Length; index++)
			{
				Transform current = childTransforms[index];
				if (current == transform)
				{
					continue;
				}

				AddTarget(current, uniqueTargets);
			}
		}

		AddTarget(transform, uniqueTargets);

		for (int index = 0; index < extraLeafTargets.Count; index++)
		{
			AddTarget(extraLeafTargets[index], uniqueTargets);
		}
	}

	private void AddTarget(Transform targetTransform, HashSet<Transform> uniqueTargets)
	{
		if (targetTransform == null || !uniqueTargets.Add(targetTransform))
		{
			return;
		}

		targets.Add(new LeafTarget
		{
			Transform = targetTransform,
			BaseRotation = targetTransform.localRotation,
			Phase = Mathf.Abs(targetTransform.GetInstanceID() * 0.173f)
		});
	}

	private void RestoreBaseRotations()
	{
		for (int index = 0; index < targets.Count; index++)
		{
			LeafTarget target = targets[index];
			if (target.Transform != null)
			{
				target.Transform.localRotation = target.BaseRotation;
			}
		}
	}
}
