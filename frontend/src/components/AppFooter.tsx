import { ExternalLink, Globe } from "lucide-react";
import { buttonVariants } from "./ui/button";

// external links shown on the right side of the footer
const FOOTER_LINKS = [
  { label: "GitHub", href: "https://github.com/shaobo-jiao/surl", icon: ExternalLink },
  { label: "Live site", href: "https://surl.shaobo.dev", icon: Globe },
];

// Footer contains the copyright notice and links to the project
const AppFooter = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="w-full border-t">
      <div className="mx-auto flex flex-col sm:flex-row sm:justify-between items-center w-full max-w-5xl gap-2 p-4">
        <p className="text-sm text-muted-foreground">
          © {currentYear} Shaobo Jiao · Built with ASP.NET Core Web API & React
        </p>

        <nav className="flex flex-wrap items-center justify-center gap-x-2 gap-y-1">
          {
            FOOTER_LINKS.map(({ label, href, icon: Icon }) => (
              <a key={label} href={href} target="_blank" rel="noreferrer"
                className={buttonVariants({ variant: "link" })}>
                <Icon /> {label}
              </a>
            ))
          }
        </nav>
      </div>
    </footer>
  );
};

export default AppFooter;
