using System;
using System.Linq;
using System.Reflection;

namespace AlgoLesson7
{
    public static class Lesson7AlgorithmFactory
    {
        public static ISynergyCalculator CreateSynergyCalculator()
        {
            return CreateImplementation<ISynergyCalculator>() ?? new EmptySynergyCalculator();
        }

        public static ITargetSelector CreateTargetSelector()
        {
            return CreateImplementation<ITargetSelector>() ?? new FirstAliveTargetSelector();
        }

        public static IHexPathfinder CreateHexPathfinder()
        {
            return CreateImplementation<IHexPathfinder>() ?? new GreedyHexPathfinder();
        }

        private static T CreateImplementation<T>() where T : class
        {
            Type contract = typeof(T);

            Type implementation = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetTypesSafely)
                .FirstOrDefault(type =>
                    contract.IsAssignableFrom(type) &&
                    !type.IsInterface &&
                    !type.IsAbstract &&
                    type.Namespace == "AlgoLesson7.Student");

            if (implementation == null)
                return null;

            return Activator.CreateInstance(implementation) as T;
        }

        private static Type[] GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null).ToArray();
            }
        }

        private sealed class EmptySynergyCalculator : ISynergyCalculator
        {
            public SynergyResult Calculate(System.Collections.Generic.IReadOnlyList<UnitSnapshot> allies)
            {
                return SynergyResult.Empty;
            }
        }

        private sealed class FirstAliveTargetSelector : ITargetSelector
        {
            public int SelectTarget(
                UnitSnapshot self,
                System.Collections.Generic.IReadOnlyList<UnitSnapshot> enemies)
            {
                if (enemies == null)
                    return -1;

                foreach (UnitSnapshot enemy in enemies)
                {
                    if (enemy.IsAlive)
                        return enemy.Id;
                }

                return -1;
            }
        }

        private sealed class GreedyHexPathfinder : IHexPathfinder
        {
            public AutoChessBoardCell FindNextStep(
                AutoChessBoardCell start,
                AutoChessBoardCell target,
                System.Collections.Generic.IReadOnlyList<AutoChessBoardCell> cells)
            {
                AutoChessBoardCell best = null;
                int bestDistance = HexGridUtility.GetDistance(start, target);

                foreach (AutoChessBoardCell neighbor in HexGridUtility.GetOpenNeighbors(start, cells))
                {
                    int distance = HexGridUtility.GetDistance(neighbor, target);

                    if (distance < bestDistance)
                    {
                        best = neighbor;
                        bestDistance = distance;
                    }
                }

                return best;
            }
        }
    }
}
