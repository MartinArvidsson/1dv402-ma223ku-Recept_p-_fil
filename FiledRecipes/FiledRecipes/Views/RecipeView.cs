using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView // Publik klass som heter RecipeView, ärver ifrån Viewbase och Interfacet IRecipeView
    {
        private const string ingredienser = "Ingredienser";
        private const string instruktioner = "Instruktioner";
        public void Show(IRecipe recipe) //Visar 1 recept
        {
            Header = recipe.Name; // Header blir receptets namn
            ShowHeaderPanel(); // Visar headern
            Console.WriteLine("");
            Console.WriteLine("============");
            Console.WriteLine(ingredienser);//Skriv ut ingredienserna
            Console.WriteLine("============");
            foreach(IIngredient ingredient in recipe.Ingredients) // För varje ingrediens i recipie.Ingredients
            {
                Console.WriteLine(ingredient); // Skriv ut ingrediensen
            }
            int instructionPart = 1; // Sätter variablen instructionPart till 1
            Console.WriteLine("");
            Console.WriteLine("==============");
            Console.WriteLine(instruktioner); // Skriver ut instruktionerna
            Console.WriteLine("=============="); // Skriver ut instruktionerna
            foreach(string instruction in recipe.Instructions) // För varje string /rad i recipe.Instruktions
            {
                Console.WriteLine("{0}.\n {1}", instructionPart,instruction); //Skriv ut instructionPart, sedan instruktionen
                instructionPart++; // Plussa på med 1 
            }
        }
        public void Show(IEnumerable<IRecipe> recipes) // Visar alla recept.
        {
            foreach(IRecipe recipe in recipes) // För varje recept i recept
            {
                Show(recipe); // Visa recept ( Gör rad 15 till 31)
                ContinueOnKeyPressed(); // Gå vidare på tangenttryck till nästa recept
            }
        }
    }
}
