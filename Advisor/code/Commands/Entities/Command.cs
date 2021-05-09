﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Advisor.Commands.Attributes;
using Advisor.Enums;

namespace Advisor.Commands.Entities
{
    /// <summary>
    /// Represents a command.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Returns the name of this command.
        /// Set using the <see cref="Advisor.Commands.Attributes.CommandAttribute"/>.
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// Returns the full name of this command assembled with the module's prefix.
        /// I.e. the "list" command in a module with the "permissions" prefix would be "permissions list".
        /// </summary>
        public string FullName { get; internal set; }

        /// <summary>
        /// Returns the possible aliases of this command.
        /// Set using the <see cref="Advisor.Commands.Attributes.AliasAttribute"/>.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; }
        
        /// <summary>
        /// Returns the description of this command.
        /// Set using the <see cref="Advisor.Commands.Attributes.DescriptionAttribute"/>.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Whether or not this command should be visible by autocompletion and user facing UI.
        /// </summary>
        public bool IsHidden => CommandAttribute.IsHidden;

        /// <summary>
        /// If set, the caller's permission level will be checked against the target(s) permission level(s) to see if they can be targeted.
        /// </summary>
        public TargetPermission? TargetPermissionLevel => CommandAttribute.TargetPermissionLevel;

        /// <summary>
        /// The realm this command should be executed on.
        /// </summary>
        public SandboxRealm ExecutionRealm => CommandAttribute.ExecutionRealm;
        
        /// <summary>
        /// The module this command came from.
        /// </summary>
        public CommandModule ParentModule { get; internal set; }

        /// <summary>
        /// The category this command belongs to, set in the ParentModule.
        /// If empty or invalid, this command will be under 'Uncategorized'
        /// </summary>
        public string Category => ParentModule?.Category ?? "Uncategorized";
        
        /// <summary>
        /// The arguments of this command.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments { get; internal set; }
        
        /// <summary>
        /// The command attribute that was placed on the method.
        /// </summary>
        internal CommandAttribute CommandAttribute { get; set; }

        /// <summary>
        /// The method this command was created from.
        /// </summary>
        internal MethodInfo Method { get; set; }

        /// <summary>
        /// Dynamically generated delegate from the command's MethodInfo.
        /// </summary>
        internal Delegate MethodDelegate { get; set; }
        
        /// <summary>
        /// Dynamically generated delegate for commands with no more arguments than the CommandContext.
        /// Offers faster execution.
        /// </summary>
        internal Action<CommandContext> MethodDelegateNoParams { get; set; }

        internal void ExecuteCommand(object[] args)
        {
            if (args.Length == 0 || args[0] is not CommandContext ctx)
            {
                throw new ArgumentException("Args must have a CommandContext as its first argument.");
            }
            
            if (MethodDelegate != null)
            {
                MethodDelegate.DynamicInvoke(args);
            }
            else if (MethodDelegateNoParams != null)
            {
                MethodDelegateNoParams(ctx);
            }
            else
            {
                throw new InvalidOperationException($"Command '{FullName}' has no method delegate! This should never happen.");
            }
        }
    }
}