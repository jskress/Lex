## History of the Library

I've always held a certain fascination with domain-specific languages and, they being
pretty natural to me, I'd already built a few.  When a language can be designed for a
certain domain, solutions to problems in that domain tend to be more readily expressible.
You end up with better control over the solution while also making the solution more
maintainable over time since it is expressed in terms of the domain rather than something
general.

The one key lesson I learned is that it is far easier to infer meaning from source code
(in a programmatic sense, anyway) if you separate the parsing into two passes, a lexical
pass and then the semantic one.  Every time I didn't, things got horribly complicate.
Given the current state of our industry, this shouldn't come as a surprise, but back then
it was a revelation.

Back in the 90's, I worked on a data migration project which needed to be able to understand
data definitions from a variety of sources; everything from C `struct`s to SQL DDL.  What
they all had in common was that they were all programming languages of some form or other.
So, I decided to apply what I'd learned and developed from DSLs to the problem and, voilà!
Lex was born.

It was originally written and maintained in Java.  I've been fixing, enhancing and fine-tuning
it ever since.  Over the past several years my language of choice has shifted to C# and so
now, I've finally decided to bring Lex along for the ride; it's just too useful to leave
behind.
