using UnityEngine;
using System.Collections.Generic;

public static class BotNameUtility
{
    private static readonly string[] SpanishNames = {
        "Andrés", "Beatriz", "Carlos", "Diana", "Enrique", "Fabiola", "Gonzalo", "Helena",
        "Ignacio", "Julia", "Kevin", "Laura", "Mateo", "Natalia", "Óscar", "Patricia",
        "Ricardo", "Sofía", "Tomás", "Úrsula", "Vicente", "Ximena", "Yago", "Zoe",
        "Paco", "Manolo", "Curro", "Lola", "Carmen", "Javi", "Dani", "Marta",
        "Roberto", "Sara", "Tomás", "Raquel", "Hugo", "Paula", "Marcos", "Lucía",
        "Aitor", "Nerea", "Iker", "Amaia", "Santi", "Alba", "Jose", "Pilar"
    };

    private static List<string> unusedNames = new List<string>();

    public static string GetRandomName()
    {
        if (unusedNames.Count == 0)
        {
            unusedNames.AddRange(SpanishNames);
            // Mezclar lista (Shuffle)
            for (int i = 0; i < unusedNames.Count; i++) {
                string temp = unusedNames[i];
                int randomIndex = Random.Range(i, unusedNames.Count);
                unusedNames[i] = unusedNames[randomIndex];
                unusedNames[randomIndex] = temp;
            }
        }

        string name = unusedNames[0];
        unusedNames.RemoveAt(0);
        return name;
    }
}
