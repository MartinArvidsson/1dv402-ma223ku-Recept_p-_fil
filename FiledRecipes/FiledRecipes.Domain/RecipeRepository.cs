using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        public void Load()
        {
            List<IRecipe> recipes = new List<IRecipe>();
            Recipe fullRecipe = null;
            RecipeReadStatus recipeReadStatus = new RecipeReadStatus();
            using (StreamReader reader = new StreamReader(_path)) //öppnar textfilen för att läsas
            {
                string line;
                while ((line = reader.ReadLine()) !=null)
                {
                    switch(line)
                    {
                        case SectionRecipe: // SectionRecipe / ingredients / instruktions är konstanter skapade innan.
                            recipeReadStatus = RecipeReadStatus.New;
                            continue;
                        case SectionIngredients:
                            recipeReadStatus = RecipeReadStatus.Ingredient;
                            continue;
                        case SectionInstructions:
                            recipeReadStatus = RecipeReadStatus.Instruction;
                            continue;
                    }
                    if (line !="")
                    {
                        switch (recipeReadStatus)
                        {
                            case RecipeReadStatus.New: // Skapar ett nytt receptobjekt med receptets namn.
                                fullRecipe = new Recipe(line);
                                recipes.Add(fullRecipe);
                                break;
                            case RecipeReadStatus.Ingredient: //Delar upp texten genom att använda split vid ; i stringklassen
                                string[] ingredients = line.Split(new string[] { ";" }, StringSplitOptions.None);
                                if (ingredients.Length % 3 !=0) // Om inte talet blir 3 kastas undantag
                                {
                                    throw new FileFormatException();
                                }//Skapar ett objekt med ingredienser och visar i mängd mått o namn.
                                Ingredient ingredient = new Ingredient();
                                ingredient.Amount = ingredients[0]; // 0 för att mängden ska skrivas först
                                ingredient.Measure = ingredients[1]; // 1 för måttet
                                ingredient.Name = ingredients[2];  // för namnet.
                                fullRecipe.Add(ingredient);
                                break;
                            case RecipeReadStatus.Instruction: // Lägger till instruktionerna
                                fullRecipe.Add(line);
                                break;
                            case RecipeReadStatus.Indefinite: // Blir ngt fel kastas undantaget nedan.
                                throw new FileFormatException();
                        }
                    }
                }
                recipes.TrimExcess(); // tar bort det tomma i arrayen.
                _recipes = recipes.OrderBy(recipe => recipe.Name).ToList(); // Sorterar listan baserad på namnen. Tilldelar en referens till listan i  fältet i _recipes.
                IsModified = false; // tilldelar att listan är oförändrad.
                OnRecipesChanged(EventArgs.Empty); //när receptet startas skickas detta med
            }
        }
        public void Save() // Koden nedan är för att man ska kunna spara recept.
        {
            using (StreamWriter writer = new StreamWriter(_path))
            {
                foreach (Recipe recipe in _recipes) // För varje recept i _recipes skriv recepten.
                {
                    writer.WriteLine(SectionRecipe); // SKriver recptets namn
                    writer.WriteLine(recipe.Name);
                    writer.WriteLine(SectionIngredients); // Skriver ingredienserna.
                    foreach(Ingredient ingredient in recipe.Ingredients)
                    {
                        writer.WriteLine("{0};{1};{2}", ingredient.Amount, ingredient.Measure, ingredient.Name); // 0 = mängd 1 = mått 2  namn
                    }
                    writer.WriteLine(SectionInstructions); // Skriver instruktionerna.
                    foreach (string instructions in recipe.Instructions)  // för varje instruktion i recipe.Instructions skriv instruktionerna.
                    {
                        writer.WriteLine(instructions);
                    }
                }
                IsModified = false;
                OnRecipesChanged(EventArgs.Empty);
            }
        }
    }
}
