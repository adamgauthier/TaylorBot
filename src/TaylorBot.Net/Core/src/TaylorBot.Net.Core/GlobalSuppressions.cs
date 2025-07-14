// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "False positive, thread-safety optimization", Scope = "member", Target = "~M:TaylorBot.Net.Core.Tasks.SingletonTaskRunner.RunTaskIfNotRan(System.Func{System.Threading.Tasks.Task},System.String)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Class explicitly made to not be cryptographically safe", Scope = "member", Target = "~M:TaylorBot.Net.Core.Random.PseudoRandom.GetInt32Exclusive(System.Int32,System.Int32)~System.Int32")]
