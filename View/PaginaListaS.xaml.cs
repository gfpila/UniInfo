using Extensionista.Models;
using Extensionista.Repositories;

namespace Extensionista.View;

public partial class PaginaListaS : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Atualiza o estado de favoritos sempre que a p�gina aparecer
        AtualizarListaFavoritos();
    }

    public PaginaListaS(int idUniversidade, string municipio)
	{
		InitializeComponent();
        CarregarCursosS(idUniversidade, municipio);
        NavigationPage.SetHasNavigationBar(this, false);

        MessagingCenter.Subscribe<PaginaFavoritos>(this, "AtualizarFavoritos", (sender) =>
            {
                AtualizarListaFavoritos();
            });
    }

    private async void sairListaS_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCursoSSelected(object sender, TappedEventArgs e)
    {
        if (sender is Element element && element.BindingContext is SisuCursos SelectedCurso)
        {
            try
            {
                string IdCurso = SelectedCurso.ID_CURSO;
                await Navigation.PushAsync(new PaginaCurso(IdCurso));
            }
            catch (Exception ex)
            {
                // Log ou mensagem de erro para o usu�rio
                await DisplayAlert("Erro", $"Ocorreu um erro ao verificar o SISU: {ex.Message}", "OK");
            }
        }
    }

    private void CarregarCursosS(int idUniversidade, string municipio)
    {
        var sisuRepository = new SisuCursosRepository();
        // Obtenha a universidade para obter o c�digo de IES e cidade
        var cursosGeralRepository = new CursosGeralRepository();
        var universidade = cursosGeralRepository.ObterUniversidade(idUniversidade);

        if (universidade != null)
        {
            // Obtenha os cursos do SISU usando o reposit�rio atualizado
            var cursos = sisuRepository.ObterCursosSisuCidade(universidade.CODIGO_IES.ToString(), universidade.MUNICIPIO);

            if (cursos != null && cursos.Any())
            {
                var favoritosRepository = new FavoritosRepository();
                var favoritos = favoritosRepository.ObterFavoritos();

                // Atualiza o estado de favoritado dos cursos
                foreach (var curso in cursos)
                {
                    curso.Favorito = favoritos.Any(f => f.ID.ToString() == curso.ID_CURSO);
                }

                // Adicione um �ndice para cada curso (caso necess�rio)
                for (int i = 0; i < cursos.Count; i++)
                {
                    cursos[i].Index = i;
                }

                // Define a fonte do ListView ou CollectionView
                ListaCursosS.ItemsSource = cursos;

                // Atualiza o estado de favorito da universidade
                universidade.Favorito = favoritos.Any(f => f.ID_UNIVERSIDADE == universidade.ID_UNIVERSIDADE);
                AtualizarIconeFavoritar(universidade.Favorito);

                // Atualiza o contexto de dados para vincular a universidade
                this.BindingContext = universidade;
            }
            else
            {
                // Se n�o houver cursos, mostre uma mensagem ou trate conforme necess�rio
                DisplayAlert("Aviso", "Nenhum curso encontrado para esta universidade.", "OK");
            }
        }
    }


    private void AtualizarListaFavoritos()
    {
        var favoritosRepository = new FavoritosRepository();
        var favoritos = favoritosRepository.ObterFavoritos();

        // Atualiza o estado dos favoritos na lista de cursos
        if (ListaCursosS.ItemsSource is IList<Cursos> cursos)
        {
            foreach (var curso in cursos)
            {
                curso.Favorito = favoritos.Any(f => f.ID_UNIVERSIDADE == curso.ID_UNIVERSIDADE);
            }

            // Atualiza a fonte de dados para refletir as mudan�as
            ListaCursosS.ItemsSource = null;
            ListaCursosS.ItemsSource = cursos;
        }

        // Atualiza o �cone de favoritar da universidade
        if (BindingContext is Universidades universidade)
        {
            universidade.Favorito = favoritos.Any(f => f.ID_UNIVERSIDADE == universidade.ID_UNIVERSIDADE);
            AtualizarIconeFavoritar(universidade.Favorito);
        }
    }

    private void AtualizarIconeFavoritar(bool isFavorito)
    {
        // Verifica o estado de favoritado e altera a imagem
        favoritarS.Source = isFavorito ? "fullheart.png" : "heart.png";
    }

    private void favoritarS_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is Universidades universidade)
        {
            var favoritosRepository = new FavoritosRepository();

            // Alterna o estado de favoritado
            universidade.Favorito = !universidade.Favorito;

            // Salva ou remove do banco de dados
            var favorito = new Favoritos
            {
                NOME_IES = universidade.NOME_IES,
                ID_UNIVERSIDADE = universidade.ID_UNIVERSIDADE,
                MUNICIPIO = universidade.MUNICIPIO,
            };

            if (universidade.Favorito)
            {
                favoritosRepository.Favoritar(favorito); // Salva no banco de dados
            }
            else
            {
                favoritosRepository.Delete(favorito); // Remove do banco de dados
            }

            AtualizarIconeFavoritar(universidade.Favorito);

            // Notifica outras p�ginas sobre a mudan�a
            MessagingCenter.Send(this, "AtualizarFavoritos");
        }
    }

}