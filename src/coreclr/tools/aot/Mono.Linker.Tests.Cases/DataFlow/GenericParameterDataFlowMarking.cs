﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Helpers;

namespace Mono.Linker.Tests.Cases.DataFlow
{
	[ExpectedNoWarnings]
	public class GenericParameterDataFlowMarking
	{
		public static void Main ()
		{
			NestedGenerics.Test ();
		}

		[Kept]
		class NestedGenerics
		{
			[Kept]
			interface IUse { void Use (); }

			[Kept]
			class RequiresMethods<[DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicMethods)] T> : IUse
			{
				[Kept]
				public void Use () { }
			}

			[Kept]
			class RequiresNothing<T> : IUse
			{
				[Kept]
				public void Use () { }
			}

			[Kept]
			class RequiresFields<[DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicFields)] T> : IUse
			{
				[Kept]
				public void Use () { }
			}

			[Kept]
			class GenericMethodNoReference
			{
				[Kept]
				static void GenericMethod<T> () { }

				[Kept]
				class TargetTypeForNothing
				{
					public int PublicField;
					public static void PublicMethod () { }
					static void PrivateMethod () { }
				}

				[Kept]
				class TargetType
				{
					public int PublicField;
					[Kept] // This is technically not necessary, but the complexity of the implementation would be much higher
					public static void PublicMethod () { }
					static void PrivateMethod () { }
				}

				[Kept]
				public static void Test()
				{
					GenericMethod<RequiresFields<RequiresNothing<TargetTypeForNothing>>> ();
					GenericMethod<RequiresFields<RequiresNothing<RequiresMethods<TargetType>>>> ();
				}
			}

			[Kept]
			class GenericMethodCallReference
			{
				[Kept]
				static void GenericMethod<T> (T value) where T : IUse { value.Use (); }

				[Kept]
				class TargetTypeForNothing : IUse
				{
					public int PublicField;
					public static void PublicMethod () { }
					static void PrivateMethod () { }

					public void Use () { }
				}

				[Kept]
				class TargetType : IUse
				{
					public int PublicField;
					[Kept]
					public static void PublicMethod () { }
					static void PrivateMethod () { }

					[Kept]
					public void Use () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericMethod<RequiresFields<RequiresNothing<TargetTypeForNothing>>> (null);
					GenericMethod<RequiresFields<RequiresNothing<RequiresMethods<TargetType>>>> (null);
				}
			}

			[Kept]
			class GenericMethodReflectionReference
			{
				[Kept]
				static void GenericMethod<T> () { _ = typeof (T).Name; }

				[Kept]
				class TargetTypeForNothing
				{
					public int PublicField;
					public static void PublicMethod () { }
					static void PrivateMethod () { }
				}

				[Kept]
				class TargetType
				{
					public int PublicField;
					[Kept]
					public static void PublicMethod () { }
					static void PrivateMethod () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericMethod<RequiresFields<RequiresNothing<TargetTypeForNothing>>> ();
					GenericMethod<RequiresFields<RequiresNothing<RequiresMethods<TargetType>>>> ();
				}
			}

			[Kept]
			[KeptMember(".ctor()")]
			class GenericInstanceMethod
			{
				[Kept]
				void GenericMethod<T> () { _ = typeof (T).Name; }

				[Kept]
				class TargetTypeForNothing
				{
					public static void PublicMethod () { }
				}

				[Kept]
				class TargetType
				{
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericInstanceMethod instance = new ();
					instance.GenericMethod<RequiresFields<RequiresNothing<TargetTypeForNothing>>> ();
					instance.GenericMethod<RequiresFields<RequiresFields<RequiresMethods<RequiresMethods<TargetType>>>>> ();
				}
			}

			[Kept]
			class GenericMethodOnGenericType
			{
				class GenericType<TType>
				{
					[Kept]
					public static void GenericMethod<TMethod> () { _ = typeof (TType).Name + typeof (TMethod).Name; }
				}

				[Kept]
				class TargetTypeForTType
				{
					public int Field;
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				class TargetTypeForTMethod
				{
					[Kept]
					public int Field;
					public static void PublicMethod () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericType<RequiresFields<RequiresNothing<RequiresMethods<TargetTypeForTType>>>>
						.GenericMethod<RequiresMethods<RequiresFields<RequiresFields<TargetTypeForTMethod>>>> ();
				}
			}

			[Kept]
			class MethodOnGenericType
			{
				class GenericType<TType>
				{
					[Kept]
					public static void Method () { _ = typeof (TType).Name; }
				}

				[Kept]
				class TargetTypeForTType
				{
					public int Field;
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericType<RequiresFields<RequiresNothing<RequiresMethods<TargetTypeForTType>>>>
						.Method ();
				}
			}

			[Kept]
			class FieldOnGenericType
			{
				class GenericType<TType>
				{
					// NativeAOT will not preserve any information about the type or field
					// the access to the field will be optimized as just a write to a memory location.
					[Kept (By = ProducedBy.Trimmer)]
					public static int Field;
				}

				[Kept]
				class TargetTypeForTType
				{
					public int Field;
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				public static void Test ()
				{
					GenericType<RequiresFields<RequiresNothing<RequiresMethods<TargetTypeForTType>>>>
						.Field = 0;
				}
			}

			[Kept]
			class BaseTypeGenericNesting
			{
				[Kept]
				class Base<T>
				{
				}

				[Kept]
				class TargetType
				{
					public int Field;
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				class DerivedWithTarget
					: Base<RequiresMethods<RequiresNothing<RequiresMethods<TargetType>>>>
				{ }

				[Kept]
				public static void Test ()
				{
					Type a;
					a = typeof (DerivedWithTarget);
				}
			}

			[Kept]
			class InterfaceGenericNesting
			{
				[Kept]
				class IBase<T>
				{
				}

				[Kept]
				class TargetType
				{
					public int Field;
					[Kept]
					public static void PublicMethod () { }
				}

				[Kept]
				class DerivedWithTarget
					: IBase<RequiresMethods<RequiresNothing<RequiresMethods<TargetType>>>>
				{ }

				[Kept]
				public static void Test ()
				{
					Type a;
					a = typeof (DerivedWithTarget);
				}
			}

			[Kept]
			public static void Test ()
			{
				GenericMethodNoReference.Test ();
				GenericMethodCallReference.Test ();
				GenericMethodReflectionReference.Test ();
				GenericInstanceMethod.Test ();
				GenericMethodOnGenericType.Test ();
				MethodOnGenericType.Test ();
				FieldOnGenericType.Test ();
				BaseTypeGenericNesting.Test ();
				InterfaceGenericNesting.Test ();
			}
		}
	}
}
