using Extensionista.Models;
using Extensionista.Repositories;
using Extensionista.View;
using System.Collections.ObjectModel;

namespace Extensionista
{
    public partial class PaginaPesquisa : ContentPage
    {
        public ObservableCollection<Universidades> UniversidadesList { get; set; } = new ObservableCollection<Universidades>();
        private readonly CursosGeralRepository _cursosGeralRepository;
        private readonly SisuCursosRepository _sisuCursosRepository;
        private int currentPage = 1;
        private bool hasMoreItems = true;
        private bool isLoading = false;

        public PaginaPesquisa()
        {
            InitializeComponent();
            BindingContext = this;
            _cursosGeralRepository = new CursosGeralRepository();
            _sisuCursosRepository = new SisuCursosRepository();
            ListaFaculdades.ItemsSource = UniversidadesList;
            NavigationPage.SetHasNavigationBar(this, false);

            // Carregar as universidades ao iniciar, sem filtro
            _ = LoadFaculdadesAsync();
        }

        // Carrega a lista de faculdades com pagina��o e filtros opcionais
        private async Task LoadFaculdadesAsync(int? codigoIES = null, string municipio = null, string nome = null)
        {
            if (isLoading) return;

            isLoading = true;

            try
            {
                // Obt�m as universidades com base nos filtros e na p�gina atual
                var universidades = await Task.Run(() =>
                    _cursosGeralRepository.ObterUniversidades(codigoIES, municipio, nome, currentPage));

                // Verifica se h� mais itens a carregar
                if (universidades.Count > 0)
                {
                    foreach (var universidade in universidades)
                    {
                        UniversidadesList.Add(universidade);
                    }

                    if (universidades.Count == 20) // Se o n�mero de resultados for 20, h� mais p�ginas
                    {
                        currentPage++;
                        hasMoreItems = true;
                    }
                    else
                    {
                        hasMoreItems = false; // N�o h� mais p�ginas
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar universidades: {ex.Message}");
                await DisplayAlert("Erro", "N�o foi poss�vel carregar os dados.", "OK");
            }
            finally
            {
                isLoading = false;
            }
        }

        // M�todo acionado ao alcan�ar o limite de rolagem, carregando a pr�xima p�gina
        private async void OnRemainingItemsThresholdReached(object sender, EventArgs e)
        {
            if (!hasMoreItems || isLoading) return;

            // Carrega mais itens com base na pesquisa atual (se houver)
            await LoadFaculdadesAsync(nome: entrySearch.Text?.ToLower());
        }

        // M�todo de pesquisa otimizado para usar filtros diretamente no reposit�rio
        private async void Pesquisar_Clicked(object sender, EventArgs e)
        {
            var query = entrySearch.Text?.ToLower();
  
            if (!string.IsNullOrEmpty(query))
            {
                // Limpa a lista atual para exibir somente os resultados da pesquisa
                UniversidadesList.Clear();

                try
                {
                    ClearIcon.IsVisible = true;
                    // Executa a consulta SQL personalizada no reposit�rio
                    var universidades = _cursosGeralRepository.ExecutarQueryPersonalizada<Universidades>(
                        "SELECT * FROM Universidades WHERE LOWER(MUNICIPIO) LIKE ? OR LOWER(NOME_IES) LIKE ?",
                        $"%{query}%", $"%{query}%"
                    );

                    // Adiciona os resultados na lista observ�vel
                    foreach (var universidade in universidades)
                    {
                        UniversidadesList.Add(universidade);
                    }

                    // Feedback para o usu�rio se n�o houver resultados
                    if (UniversidadesList.Count == 0)
                    {
                        await DisplayAlert("Nenhum resultado", "Nenhuma universidade encontrada para os crit�rios de busca.", "OK");
                    }

                    // Rola a lista para o in�cio
                    if (UniversidadesList.Count > 0)
                    {
                        ListaFaculdades.ScrollTo(UniversidadesList[0], position: ScrollToPosition.Start, animate: true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro na query direta: {ex.Message}");
                }
                ClearIcon.IsVisible = true;
            }
            else
            {
                // Caso o campo de pesquisa esteja vazio, recarrega todos os dados
                UniversidadesList.Clear();
                currentPage = 1; // Reinicia a pagina��o
                hasMoreItems = true; // Habilita mais itens para rolagem
                ClearIcon.IsVisible = false;
                await LoadFaculdadesAsync();

                // Rola a lista para o in�cio
                if (UniversidadesList.Count > 0)
                {
                    ListaFaculdades.ScrollTo(UniversidadesList[0], position: ScrollToPosition.Start, animate: true);
                }
            }
        }

        // M�todo para manipular a sele��o de um item na lista
        private async void OnFaculdadeSelected(object sender, EventArgs e)
        {
            if (sender is Element element && element.BindingContext is Universidades selectedFaculdade)
            {
                try
                {
                    var cursosSisu = _sisuCursosRepository.ObterCursosSisuCidade(selectedFaculdade.CODIGO_IES.ToString(), selectedFaculdade.MUNICIPIO);
                    bool estaNoSisu = cursosSisu.Any();

                    if (estaNoSisu)
                    { 
                        await Navigation.PushAsync(new PaginaListaS(selectedFaculdade.ID_UNIVERSIDADE, selectedFaculdade.MUNICIPIO));
                    }
                    else
                    {
                        await Navigation.PushAsync(new PaginaLista(selectedFaculdade.ID_UNIVERSIDADE, estaNoSisu));
                    }
                }
                catch (Exception ex)
                {
                    // Log ou mensagem de erro para o usu�rio
                    await DisplayAlert("Erro", $"Ocorreu um erro ao verificar o SISU: {ex.Message}", "OK");
                }
            }
        }

        private async void ClearSearch_Clicked(object sender, EventArgs e)
        {
            entrySearch.Text = string.Empty; // Limpa o texto da entrada
            UniversidadesList.Clear(); // Limpa os resultados atuais
            currentPage = 1; // Reinicia a pagina��o
            hasMoreItems = true; // Habilita mais itens para rolagem
            await LoadFaculdadesAsync(); // Carrega a lista completa
            ClearIcon.IsVisible = false; // Oculta o bot�o de limpar
        }

    }
}