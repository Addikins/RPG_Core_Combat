using System;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        [SerializeField] GameObject levelUpEffect = null;
        [SerializeField] bool shouldUseModifiers = false;
        [SerializeField] UnityEvent levelUpEvent;

        public event Action onLevelUp;

        Experience experience;

        LazyValue<int> currentLevel;


        private void Awake()
        {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start()
        {
            currentLevel.ForceInit();
            currentLevel.value = CalculateLevel();
        }

        private void OnEnable()
        {
            if (experience != null)
            {
                // Subscribing UpdateLevel to onExperienceGained
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                // Subscribing UpdateLevel to onExperienceGained
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                // print("Leveled Up!");
                LevelUpEffect();
                onLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            levelUpEvent.Invoke();
            Instantiate(levelUpEffect, transform);
        }

        public CharacterClass GetCharacterClass()
        {
            return characterClass;
        }

        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);
        }

        public int GetLevel()
        {
            return currentLevel.value;
        }

        public float ExpToNextLevelUp()
        {
            return progression.GetStat(Stat.ExperienceToLevelUp, characterClass, GetLevel());
        }

        public float GetExpProgression()
        {
            if (GetLevel() == 1)
            {
                return (experience.GetExperiencePoints() / ExpToNextLevelUp());
            }
            return ((experience.GetExperiencePoints() - LastExpToLevel()) / (ExpToNextLevelUp() - LastExpToLevel()));
        }

        private float LastExpToLevel()
        {
            return progression.GetStat(Stat.ExperienceToLevelUp, characterClass, GetLevel() - 1);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        private float GetPercentageModifier(Stat stat)
        {
            if (!shouldUseModifiers) { return 0; }

            float totalModification = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    totalModification += modifier;
                }
            }
            return totalModification;
        }

        private float GetAdditiveModifier(Stat stat)
        {
            if (!shouldUseModifiers) { return 0; }

            float totalModification = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    totalModification += modifier;
                }
            }
            return totalModification;
        }

        private int CalculateLevel()
        {

            Experience experience = GetComponent<Experience>();
            if (experience == null) return startingLevel;

            float currentXP = experience.GetExperiencePoints();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float xpToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (xpToLevelUp > currentXP)
                {
                    return level;
                }
            }
            return penultimateLevel + 1;
        }
    }
}