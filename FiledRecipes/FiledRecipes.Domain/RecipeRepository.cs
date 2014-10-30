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
        public void Load() // Public void som heter Load
        {
            List<IRecipe> recipes = new List<IRecipe>(); // Skapar ett listobjekt av interfacet <IRecipe>
            Recipe aRecipe = null; // aRecipe är = 0
            RecipeReadStatus recipeReadStatus = new RecipeReadStatus(); // Nytt objekt som heter recipeReadStatus skapas av RecipeReadStatus.  
            using (StreamReader reader = new StreamReader(_path)) //öppnar textfilen för att läsas via _path
            {
                string line; // Skapar en stringvariabel med namnet line.
                while ((line = reader.ReadLine()) != null) //När programmet läser o raden INTE är tom:
                {
                    switch(line) //går hit
                    {
                        case SectionRecipe: // Rad 16
                            recipeReadStatus = RecipeReadStatus.New; // Öppnar nytt för att läsa.
                            continue; //Fortsätter och går tillbaka till while loopen
                        case SectionIngredients: // Rad 21
                            recipeReadStatus = RecipeReadStatus.Ingredient; // Öppnar nytt för att läsa ingredienserna 
                            continue;//fortsätter och går tillbaka till while loopen
                        case SectionInstructions: // Rad 26
                            recipeReadStatus = RecipeReadStatus.Instruction; //Öppnar nytt för att läsa instruktionerna
                            continue; //Fortsätter och går tillbaka till while loopen
                    }
                    if (line != "") // Om raden INTE är tom
                    {
                        switch (recipeReadStatus) // Gör sig "redo" för att läsa
                        {
                            case RecipeReadStatus.New: // Skapar ett nytt receptobjekt med receptets namn.
                                aRecipe = new Recipe(line); // aRecipe får värdet som (line) just nu håller.
                                recipes.Add(aRecipe); // Lägger till lines värde via aRecipe till recipies via .add
                                break; // bryter och går tillbaka till while
                            case RecipeReadStatus.Ingredient: //Delar upp texten genom att använda split vid ; i stringklassen
                                string[] ingredients = line.Split(';'); //Skapar en string-array för ingredienserna. Delar upp 4,5;dl;filmjölk efter ;
                                if (ingredients.Length % 3 !=0) // Om inte Arrayen  blir 3 lång kastas ett undantag
                                {
                                    throw new FileFormatException(); // Undantag kastas
                                }//Skapar ett objekt med ingredienser och visar i mängd mått o namn.
                                Ingredient ingredient = new Ingredient(); // Instansierar nytt ingredient objekt
                                ingredient.Amount = ingredients[0]; //  Mängden ingredienser får platsen 0 i arrayen
                                ingredient.Measure = ingredients[1]; // 1 för måttet
                                ingredient.Name = ingredients[2];  //   2 för namnet.
                                aRecipe.Add(ingredient); // aRecipe får värdet av "ingredient, vilket är [0] [1] [2]
                                break; //Bryter o går illbaka till första while och loopar tills ALLA ingredienser är tillagda.
                            case RecipeReadStatus.Instruction: // Lägger till instruktionerna
                                aRecipe.Add(line); //Värdet som gavs på rad 146 läggs till i  aRecipe via (line) och loopar tills ALLA instruktioner är tillagda.
                                break; //bryter o går tillbaka till första while
                            case RecipeReadStatus.Indefinite: //Om den läser och ngt blir fel kastas undantaget nedan.
                                throw new FileFormatException(); //undantag
                        }
                    }
                }
                _recipes = recipes.OrderBy(recipe => recipe.Name).ToList(); // Sorterar listan baserad på namnen. Tilldelar en referens till listan i  fältet i _recipes.
                IsModified = false; //Citerar PDF:en "Tilldela avsedd egenskap i klassen, IsModified, ett värde som indikerar att listan med recept 
                                    //är oförändrad." = False löser detta.
                OnRecipesChanged(EventArgs.Empty); // Citerar PDF:en "Utlös händelse om att recept har lästs in genom att anropa metoden OnRecipesChanged och 
                                                   //skicka med parametern EventArgs.Empty."
            }
        }
        public void Save() // Koden nedan är för att man ska kunna spara recept. Skapar en public void med namnet save.
        {
            using (StreamWriter writer = new StreamWriter(_path)) // Använder streamwriter och går till där .txt filen finns (_Path)
            {
                foreach (Recipe recipe in _recipes) // För varje recept i _recipes skriv recepten.
                {
                    writer.WriteLine(SectionRecipe); 
                    writer.WriteLine(recipe.Name); // Skriver receptets namn.
                    writer.WriteLine(SectionIngredients); // Skriver ingredienserna.
                    foreach(Ingredient ingredient in recipe.Ingredients) // För varje ingrediens i ingrediens
                    {
                        writer.WriteLine("{0};{1};{2}", ingredient.Amount, ingredient.Measure, ingredient.Name); // 0 = mängd 1 = mått 2  namn
                    }
                    writer.WriteLine(SectionInstructions); // Skriver instruktionerna.
                    foreach (string instructions in recipe.Instructions)  // för varje instruktion i recipe.Instructions skriv instruktionerna.
                    {
                        writer.WriteLine(instructions); //Skriver ut instruktionerna.
                    }
                }
                IsModified = false; //Citerar PDF:en "Tilldela avsedd egenskap i klassen, IsModified, ett värde som indikerar att listan med recept 
                                   //är oförändrad." = False löser detta.
                OnRecipesChanged(EventArgs.Empty); // "Utlös händelse om att recept har lästs in genom att anropa metoden OnRecipesChanged och 
                                                   //  skicka med parametern EventArgs.Empty." - citerat från PDF:en
            }
        }
    }
}
