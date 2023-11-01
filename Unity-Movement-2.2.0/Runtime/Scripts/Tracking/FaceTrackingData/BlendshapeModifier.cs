// Copyright (c) Meta Platforms, Inc. and affiliates.

using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Oculus.Movement.Tracking
{
    /// <summary>
    /// Calculates the modified blendshape weight for a facial expression.
    /// </summary>
    public class BlendshapeModifier : MonoBehaviour
    {
        /// <summary>
        /// The modifier data for a specific set of facial expressions.
        /// </summary>
        [System.Serializable]
        public class FaceExpressionModifier
        {
            /// <summary>
            /// The facial expressions that will be modified.
            /// </summary>
            [Tooltip(BlendshapeModifierTooltips.FaceExpressionModifier.FaceExpressions)]
            public OVRFaceExpressions.FaceExpression[] FaceExpressions = new OVRFaceExpressions.FaceExpression[2];

            /// <summary>
            /// The minimum clamped blendshape weight for this set of facial expressions.
            /// </summary>
            [Range(0.0f, 2.0f)]
            [Tooltip(BlendshapeModifierTooltips.FaceExpressionModifier.MinValue)]
            public float MinValue = 0.0f;

            /// <summary>
            /// The maximum clamped blendshape weight for this set of facial expressions.
            /// </summary>
            [Range(0.0f, 2.0f)]
            [Tooltip(BlendshapeModifierTooltips.FaceExpressionModifier.MaxValue)]
            public float MaxValue = 1.0f;

            /// <summary>
            /// The blendshape weight multiplier for this set of facial expressions.
            /// </summary>
            [Range(0.0f, 2.0f)]
            [Tooltip(BlendshapeModifierTooltips.FaceExpressionModifier.Multiplier)]
            public float Multiplier = 1.0f;
        }

        /// <summary>
        /// Container class used for json serialization of face expression modifiers.
        /// </summary>
        [System.Serializable]
        private class FaceExpressionModifierArray
        {
            /// <summary>
            /// The array of serialized face expression modifiers.
            /// </summary>
            public FaceExpressionModifier[] FaceExpressionModifiers;

            /// <summary>
            /// Constructor that assigns FaceExpressionModifiers.
            /// </summary>
            /// <param name="faceExpressionModifiers">The array to initialize with.</param>
            public FaceExpressionModifierArray(FaceExpressionModifier[] faceExpressionModifiers)
            {
                FaceExpressionModifiers = faceExpressionModifiers;
            }
        }

        /// <inheritdoc cref="_faceExpressionModifiers"/>
        public IReadOnlyCollection<FaceExpressionModifier> Modifiers => _faceExpressionModifiers;

        /// <inheritdoc cref="_faceExpressionModifierMap"/>
        public IReadOnlyDictionary<OVRFaceExpressions.FaceExpression, FaceExpressionModifier>
            FaceExpressionModifierMap => _faceExpressionModifierMap;
        public Dictionary<OVRFaceExpressions.FaceExpression, bool> FaceExpressionMap;
        public Dictionary<OVRFaceExpressions.FaceExpression, bool> UpperLowerMap;

        [SerializeField]
        private ModifierModel modifierModel = ModifierModel.Default;

        /// <summary>
        /// The array of facial expression modifier data to be used.
        /// </summary>
        [SerializeField]
        [Tooltip(BlendshapeModifierTooltips.FaceExpressionsModifiers)]
        protected FaceExpressionModifier[] _faceExpressionModifiers;

        /// <summary>
        /// Optional text asset containing the array of face expression modifier data to be used.
        /// </summary>
        [SerializeField]
        [Optional]
        [Tooltip(BlendshapeModifierTooltips.DefaultBlendshapeModifierPreset)]
        protected TextAsset _defaultBlendshapeModifierPreset;

        /// <summary>
        /// Global blendshape multiplier.
        /// </summary>
        [SerializeField]
        [Tooltip(BlendshapeModifierTooltips.GlobalMultiplier)]
        protected float _globalMultiplier = 1.0f;

        /// <summary>
        /// Global blendshape clamp min.
        /// </summary>
        [SerializeField]
        [Tooltip(BlendshapeModifierTooltips.GlobalMin)]
        protected float _globalClampMin = 0.0f;

        /// <summary>
        /// Global blendshape clamp max.
        /// </summary>
        [SerializeField]
        [Tooltip(BlendshapeModifierTooltips.GlobalMax)]
        protected float _globalClampMax = 2.0f;

        /// <summary>
        /// Apply global clamping to non-mapped values.
        /// </summary>
        [SerializeField]
        [Tooltip(BlendshapeModifierTooltips.ApplyGlobalClampingNonMapped)]
        protected bool _applyGlobalClampingNonMapped = false;

        /// <summary>
        /// The dictionary mapping a OVRFaceExpressions.FaceExpression to a FaceExpressionModifier.
        /// </summary>
        private Dictionary<OVRFaceExpressions.FaceExpression, FaceExpressionModifier> _faceExpressionModifierMap;

        private void Awake()
        {
            _faceExpressionModifierMap = new Dictionary<OVRFaceExpressions.FaceExpression, FaceExpressionModifier>();
            SetupBlendshapeModifierMapping();
            if (_defaultBlendshapeModifierPreset != null)
            {
                LoadPreset(_defaultBlendshapeModifierPreset.text);
            }
            //初始化map
            FaceExpressionMap = new();
            foreach (OVRFaceExpressions.FaceExpression val in uninterstedFaceExpressions)
            {
                FaceExpressionMap.Add(val, true);
            }
            foreach (OVRFaceExpressions.FaceExpression val in interstedFaceExpressions)
            {
                FaceExpressionMap.Add(val, false);
            }
            UpperLowerMap = new();
            foreach (OVRFaceExpressions.FaceExpression val in lowerFaceExpressions)
            {
                UpperLowerMap.Add(val, true);
            }
        }

        private void SetupBlendshapeModifierMapping()
        {
            _faceExpressionModifierMap.Clear();
            foreach (var faceExpressionModifier in _faceExpressionModifiers)
            {
                Assert.IsTrue(faceExpressionModifier.FaceExpressions.Length > 0);
                foreach (var faceExpression in faceExpressionModifier.FaceExpressions)
                {
                    Assert.IsFalse(_faceExpressionModifierMap.ContainsKey(faceExpression));
                    _faceExpressionModifierMap.Add(faceExpression, faceExpressionModifier);
                }
            }
        }

        private void AddFaceExpressionModifier(OVRFaceExpressions.FaceExpression faceExpression)
        {
            Debug.LogWarning($"Missing modifier setup for {faceExpression}, creating a modifier.");
            var faceExpressionModifier = new FaceExpressionModifier
            {
                FaceExpressions = new OVRFaceExpressions.FaceExpression[2],
                MinValue = 0.0f,
                MaxValue = 1.0f,
                Multiplier = 1.0f
            };
            _faceExpressionModifierMap.Add(faceExpression, faceExpressionModifier);
        }

        private string SerializeToJson()
        {
            foreach (var faceExpressionModifier in _faceExpressionModifiers)
            {
                var currentModifier = _faceExpressionModifierMap[faceExpressionModifier.FaceExpressions[0]];
                faceExpressionModifier.MinValue = currentModifier.MinValue;
                faceExpressionModifier.MaxValue = currentModifier.MaxValue;
                faceExpressionModifier.Multiplier = currentModifier.Multiplier;
            }
            var serializedFaceExpressionModifiers =
                new FaceExpressionModifierArray(faceExpressionModifiers: _faceExpressionModifiers);
            return JsonUtility.ToJson(serializedFaceExpressionModifiers, true);
        }

        /// <summary>
        /// Returns the modified weight for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <param name="currentWeight">The unmodified weight of the facial expression.</param>
        /// <returns></returns>
        public float GetModifiedWeight(OVRFaceExpressions.FaceExpression faceExpression, float currentWeight)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                if (_applyGlobalClampingNonMapped)
                {
                    currentWeight = Mathf.Clamp(currentWeight * _globalMultiplier,
                        _globalClampMin, _globalClampMax);
                }

                return currentWeight;
            }
            var faceExpressionModifier = _faceExpressionModifierMap[faceExpression];
            float modifiedWeight = Mathf.Clamp(Mathf.Clamp(currentWeight * faceExpressionModifier.Multiplier,
                faceExpressionModifier.MinValue, faceExpressionModifier.MaxValue) * _globalMultiplier, _globalClampMin, _globalClampMax);
            
            
            //相关blend不变
            switch (modifierModel)
            {
                case ModifierModel.Default:
                    break;
                case ModifierModel.InterstedBlendshapes:
                    if (FaceExpressionMap.ContainsKey(faceExpression) && FaceExpressionMap[faceExpression] == true)
                    {
                        modifiedWeight = Mathf.Clamp(currentWeight * faceExpressionModifier.Multiplier,
                        faceExpressionModifier.MinValue, faceExpressionModifier.MaxValue);
                    }
                    break;
                case ModifierModel.LowerFace:
                    if (!UpperLowerMap.ContainsKey(faceExpression))
                    {
                        modifiedWeight = Mathf.Clamp(currentWeight * faceExpressionModifier.Multiplier,
                        faceExpressionModifier.MinValue, faceExpressionModifier.MaxValue);
                    }
                    break;
                case ModifierModel.UpperFace:
                    if (UpperLowerMap.ContainsKey(faceExpression))
                    {
                        modifiedWeight = Mathf.Clamp(currentWeight * faceExpressionModifier.Multiplier,
                        faceExpressionModifier.MinValue, faceExpressionModifier.MaxValue);
                    }
                    break;
                default:
                    break;
            }

            //修复眼睛眨动的问题
            if (faceExpression.ToString().IndexOf("EyesClose") != -1 || faceExpression.ToString().IndexOf("EyesLookDown") != -1)
            {
                modifiedWeight = Mathf.Clamp(currentWeight * faceExpressionModifier.Multiplier,
                        faceExpressionModifier.MinValue, faceExpressionModifier.MaxValue);
            }
            //使preset眼神不奇怪
            if (faceExpression.ToString().IndexOf("EyesLook") != -1)
            {
                modifiedWeight = 0;
            }
            return modifiedWeight;
        }

        /// <summary>
        /// Update the minimum clamped value for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <param name="val">The updated minimum value for the facial expression.</param>
        public void UpdateMinValue(OVRFaceExpressions.FaceExpression faceExpression, float val)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            _faceExpressionModifierMap[faceExpression].MinValue = val;
            if (faceExpression == OVRFaceExpressions.FaceExpression.Max)
            {
                _globalClampMin = val;
            }
        }

        /// <summary>
        /// Update the maximum clamped value for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <param name="val">The updated maximum value for the facial expression.</param>
        public void UpdateMaxValue(OVRFaceExpressions.FaceExpression faceExpression, float val)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            _faceExpressionModifierMap[faceExpression].MaxValue = val;
            if (faceExpression == OVRFaceExpressions.FaceExpression.Max)
            {
                _globalClampMax = val;
            }
        }

        /// <summary>
        /// Update the multiplier value for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <param name="val">The updated multiplier value for the facial expression.</param>
        public void UpdateMultiplierValue(OVRFaceExpressions.FaceExpression faceExpression, float val)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            _faceExpressionModifierMap[faceExpression].Multiplier = val;
            if (faceExpression == OVRFaceExpressions.FaceExpression.Max)
            {
                _globalMultiplier = val;
            }
        }

        /// <summary>
        /// Returns the multiplier value for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <returns>Multiplier modifier for a facial expression.</returns>
        public float GetMultiplierValue(OVRFaceExpressions.FaceExpression faceExpression)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            return _faceExpressionModifierMap[faceExpression].Multiplier;
        }

        /// <summary>
        /// Returns the minimum clamped value for a facial expression.
        /// </summary>
        /// <param name="faceExpression">The facial expression.</param>
        /// <returns>Minimum clamped value for a facial expression.</returns>
        public float GetMinValue(OVRFaceExpressions.FaceExpression faceExpression)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            return _faceExpressionModifierMap[faceExpression].MinValue;
        }

        /// <summary>
        /// Returns the maximum clamped value for a facial expression.
        /// </summary>
        /// <param name="faceExpression"></param>
        /// <returns>Maximum clamped value for a facial expression.</returns>
        public float GetMaxValue(OVRFaceExpressions.FaceExpression faceExpression)
        {
            if (!_faceExpressionModifierMap.ContainsKey(faceExpression))
            {
                AddFaceExpressionModifier(faceExpression);
            }
            return _faceExpressionModifierMap[faceExpression].MaxValue;
        }

        /// <summary>
        /// Saves the current facial expression modifiers to a timestamped json file.
        /// </summary>
        public void SavePreset()
        {
            var saveJson = SerializeToJson();
            System.IO.File.WriteAllText($"{Application.persistentDataPath}/{System.DateTime.Now:yyyyMMddTHHmmss}.json", saveJson);
        }

        /// <summary>
        /// Loads the facial expression modifiers from text.
        /// </summary>
        /// <param name="presetJson">The json containing the serialized facial expression modifiers.</param>
        public void LoadPreset(string presetJson)
        {
            var faceExpressionModifierArray = JsonUtility.FromJson<FaceExpressionModifierArray>(presetJson);
            _faceExpressionModifiers = faceExpressionModifierArray.FaceExpressionModifiers;
            SetupBlendshapeModifierMapping();
        }

        public float GetGlobalMultiplier()
        {
            return _globalMultiplier;
        }

        private readonly OVRFaceExpressions.FaceExpression[] interstedFaceExpressions = new OVRFaceExpressions.FaceExpression[] 
        {
            OVRFaceExpressions.FaceExpression.BrowLowererL,
            OVRFaceExpressions.FaceExpression.BrowLowererR,
            OVRFaceExpressions.FaceExpression.CheekPuffL,
            OVRFaceExpressions.FaceExpression.CheekPuffR,
            OVRFaceExpressions.FaceExpression.CheekRaiserL,
            OVRFaceExpressions.FaceExpression.CheekRaiserR,
            OVRFaceExpressions.FaceExpression.CheekSuckL,
            OVRFaceExpressions.FaceExpression.CheekSuckR,
            OVRFaceExpressions.FaceExpression.ChinRaiserB,
            OVRFaceExpressions.FaceExpression.ChinRaiserT,
            OVRFaceExpressions.FaceExpression.DimplerL,
            OVRFaceExpressions.FaceExpression.DimplerR,
            OVRFaceExpressions.FaceExpression.InnerBrowRaiserL,
            OVRFaceExpressions.FaceExpression.InnerBrowRaiserR,
            OVRFaceExpressions.FaceExpression.LipCornerDepressorL,
            OVRFaceExpressions.FaceExpression.LipCornerDepressorR,
            OVRFaceExpressions.FaceExpression.LipCornerPullerL,
            OVRFaceExpressions.FaceExpression.LipCornerPullerR,
            OVRFaceExpressions.FaceExpression.LipPressorL,
            OVRFaceExpressions.FaceExpression.LipPressorR,
            OVRFaceExpressions.FaceExpression.LipStretcherL,
            OVRFaceExpressions.FaceExpression.LipStretcherR,
            OVRFaceExpressions.FaceExpression.LipTightenerL,
            OVRFaceExpressions.FaceExpression.LipTightenerR,
            OVRFaceExpressions.FaceExpression.LipsToward,
            OVRFaceExpressions.FaceExpression.LowerLipDepressorL,
            OVRFaceExpressions.FaceExpression.LowerLipDepressorR,
            OVRFaceExpressions.FaceExpression.NoseWrinklerL,
            OVRFaceExpressions.FaceExpression.NoseWrinklerR,
            OVRFaceExpressions.FaceExpression.OuterBrowRaiserL,
            OVRFaceExpressions.FaceExpression.OuterBrowRaiserR,
            OVRFaceExpressions.FaceExpression.UpperLipRaiserL,
            OVRFaceExpressions.FaceExpression.UpperLipRaiserR,
            OVRFaceExpressions.FaceExpression.UpperLidRaiserL,
            OVRFaceExpressions.FaceExpression.UpperLidRaiserR,

        };
        private readonly OVRFaceExpressions.FaceExpression[] lowerFaceExpressions = new OVRFaceExpressions.FaceExpression[]
        {
            OVRFaceExpressions.FaceExpression.JawDrop,
            OVRFaceExpressions.FaceExpression.JawSidewaysLeft,
            OVRFaceExpressions.FaceExpression.JawSidewaysRight,
            OVRFaceExpressions.FaceExpression.JawThrust,
            OVRFaceExpressions.FaceExpression.DimplerL,
            OVRFaceExpressions.FaceExpression.DimplerR,
            OVRFaceExpressions.FaceExpression.LipCornerDepressorL,
            OVRFaceExpressions.FaceExpression.LipCornerDepressorR,
            OVRFaceExpressions.FaceExpression.LipCornerPullerL,
            OVRFaceExpressions.FaceExpression.LipCornerPullerR,
            OVRFaceExpressions.FaceExpression.LipFunnelerLB,
            OVRFaceExpressions.FaceExpression.LipFunnelerLT,
            OVRFaceExpressions.FaceExpression.LipFunnelerRB,
            OVRFaceExpressions.FaceExpression.LipFunnelerRT,
            OVRFaceExpressions.FaceExpression.LipPressorL,
            OVRFaceExpressions.FaceExpression.LipPressorR,
            OVRFaceExpressions.FaceExpression.LipPuckerL,
            OVRFaceExpressions.FaceExpression.LipPuckerR,
            OVRFaceExpressions.FaceExpression.LipStretcherL,
            OVRFaceExpressions.FaceExpression.LipStretcherR,
            OVRFaceExpressions.FaceExpression.LipSuckLB,
            OVRFaceExpressions.FaceExpression.LipSuckLT,
            OVRFaceExpressions.FaceExpression.LipSuckRB,
            OVRFaceExpressions.FaceExpression.LipSuckRT,
            OVRFaceExpressions.FaceExpression.LipTightenerL,
            OVRFaceExpressions.FaceExpression.LipTightenerR,
            OVRFaceExpressions.FaceExpression.LipsToward,
            OVRFaceExpressions.FaceExpression.LowerLipDepressorL,
            OVRFaceExpressions.FaceExpression.LowerLipDepressorR,
            OVRFaceExpressions.FaceExpression.MouthLeft,
            OVRFaceExpressions.FaceExpression.MouthRight,
            OVRFaceExpressions.FaceExpression.CheekPuffL,
            OVRFaceExpressions.FaceExpression.CheekPuffR,
        };
        private readonly OVRFaceExpressions.FaceExpression[] uninterstedFaceExpressions = new OVRFaceExpressions.FaceExpression[]
        {
            OVRFaceExpressions.FaceExpression.EyesClosedL,
            OVRFaceExpressions.FaceExpression.EyesClosedR,
            OVRFaceExpressions.FaceExpression.EyesLookDownL,
            OVRFaceExpressions.FaceExpression.EyesLookDownR,
            OVRFaceExpressions.FaceExpression.EyesLookLeftL,
            OVRFaceExpressions.FaceExpression.EyesLookLeftR,
            OVRFaceExpressions.FaceExpression.EyesLookRightL,
            OVRFaceExpressions.FaceExpression.EyesLookRightR,
            OVRFaceExpressions.FaceExpression.EyesLookUpL,
            OVRFaceExpressions.FaceExpression.EyesLookUpR,
            OVRFaceExpressions.FaceExpression.LidTightenerL,
            OVRFaceExpressions.FaceExpression.LidTightenerR,
            OVRFaceExpressions.FaceExpression.JawDrop,
            OVRFaceExpressions.FaceExpression.JawSidewaysLeft,
            OVRFaceExpressions.FaceExpression.JawSidewaysRight,
            OVRFaceExpressions.FaceExpression.JawThrust,
            OVRFaceExpressions.FaceExpression.MouthLeft,
            OVRFaceExpressions.FaceExpression.MouthRight,
            OVRFaceExpressions.FaceExpression.LipFunnelerLB,
            OVRFaceExpressions.FaceExpression.LipFunnelerLT,
            OVRFaceExpressions.FaceExpression.LipFunnelerRB,
            OVRFaceExpressions.FaceExpression.LipFunnelerRT,
            OVRFaceExpressions.FaceExpression.LipPuckerL,
            OVRFaceExpressions.FaceExpression.LipPuckerR,
            OVRFaceExpressions.FaceExpression.LipSuckLB,
            OVRFaceExpressions.FaceExpression.LipSuckLT,
            OVRFaceExpressions.FaceExpression.LipSuckRB,
            OVRFaceExpressions.FaceExpression.LipSuckRT,
        };

        private enum ModifierModel
        {
            Default = -1,
            InterstedBlendshapes = 1,
            UpperFace = 2,
            LowerFace = 3,
        }
    }
}
