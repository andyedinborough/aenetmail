/// runtime 4.8
///		-conservative for this. It seems if it targets 4.8, it's fine with 4.8.1
///

///This is a fork of Andy Edinborough's project. To avoid name conflicts, and to apply our devOp automation, we append a prefix. If you find this project helpful, please join us in being thankful to the original author.
/// <see cref="AssemblyInfo.cs"/> for the upstream project information.
/// 
/// as we have made changes to the project's meta (the .csproj file itself), it follows nilnul devOp pipeline, and it might be or not be merged well with other forks. As how fork works, you can merge here, there, or create your own fork.

/// as strong-named referencers need strong-named referencees, and also strong name is painful and adds no value, hence we use an arbitrary ".snk" file.
/// all our projects share the same .snk file. So we made it a symlink, pointed by a local doc inside each project. The real .snk file can be found in git repo:
///		nilnul.dev._src_.eg_.nilnul0
///			which can be found in many public git services including:
///				https://github.com/nilnul/nilnul.dev._src_.eg_.nilnul0
/// This makes our intention and methodology more obvious, and you can easily change it to your own; but we recommend keeping it, as we have previously said: its value doesnot deserve much work, and it's painful as all referencers will break if you change the strong-name.