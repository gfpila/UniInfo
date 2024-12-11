using Extensionista.View;

namespace Extensionista;

public partial class TabbedPageMenu : TabbedPage
{
    public TabbedPageMenu()
    {
        InitializeComponent();

        // Configura a p�gina de pesquisa com NavigationPage
        var paginaPesquisa = new NavigationPage(new PaginaPesquisa())
        {
            Title = "Pesquisa",
            IconImageSource = "pesquisa.png"
        };

        // Configura a p�gina de favoritos com NavigationPage
        var paginaFavoritos = new NavigationPage(new PaginaFavoritos())
        {
            Title = "Favoritos",
            IconImageSource = "favoritos.png"
        };

        var paginaIndicadores = new NavigationPage(new Indicadores())
        {
            Title = "Indicadores",
            IconImageSource = "indicadores.png"
        };

        // Adiciona as p�ginas ao TabbedPage
        Children.Add(new PaginaInicio()); // P�gina inicial n�o precisa de NavigationPage
        Children.Add(paginaPesquisa);
        Children.Add(paginaFavoritos);
        Children.Add(paginaIndicadores);
    }
}