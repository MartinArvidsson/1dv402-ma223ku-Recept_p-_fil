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
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe) //Visar 1 recept
        {
            Header = recipe.Name;
            ShowHeaderPanel();
            Console.WriteLine("\nIngredienser\n-------");
            foreach(IIngredient ingredient in recipe.Ingredients)
            {
                Console.WriteLine(ingredient);
            }
            int instructionPart = 1;
            Console.WriteLine("\nGör så här\n---------");
            foreach(string instruction in recipe.Instructions)
            {
                Console.WriteLine("<{0}>\n {1}", instructionPart,instruction);
                instructionPart++;
            }
        }
        public void Show(IEnumerable<IRecipe> recipes) // Visar alla recept.
        {
            foreach(IRecipe recipe in recipes)
            {
                Show(recipe);
                ContinueOnKeyPressed();
            }
        }
    }
}
