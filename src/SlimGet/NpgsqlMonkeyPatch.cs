// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace SlimGet
{
    public sealed class NpgsqlMonkeyPatch
    {
        public static void TryPatch()
        {
            Console.WriteLine("Attempting Npgsql patch...");

            try
            {
                var mtdct = typeof(NpgsqlCompositeMethodCallTranslator);
                var mtdts = mtdct.GetField("MethodCallTranslators", BindingFlags.Static | BindingFlags.NonPublic);

                var mtdarray = mtdts.GetValue(null) as IMethodCallTranslator[];
                var mtdarrayPatched = new IMethodCallTranslator[mtdarray.Length + 1];
                Array.Copy(mtdarray, 0, mtdarrayPatched, 0, mtdarray.Length);
                mtdarrayPatched[mtdarray.Length] = new NpgsqlTrigramMethodTranslator();
                mtdts.SetValue(null, mtdarrayPatched);

                Console.WriteLine("Nothing seems to have blown up, presumably we're good");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not patch Npgsql: {ex.GetType()}: {ex.Message}");
            }
        }
    }

    public class NpgsqlTrigramMethodTranslator : IMethodCallTranslator
    {
        private static MethodInfo Similarity { get; } =
            typeof(NpgsqlTrigramFunctions).GetMethod(nameof(NpgsqlTrigramFunctions.Similarity), BindingFlags.Public | BindingFlags.Static);

        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method != Similarity)
                return null;

            return new SqlFunctionExpression("similarity", typeof(double), e.Arguments.Skip(1));
        }
    }

    public static class NpgsqlTrigramFunctions
    {
        public static double Similarity(this DbFunctions _, string matchExpression, string pattern)
            => throw new NotImplementedException();
    }
}
