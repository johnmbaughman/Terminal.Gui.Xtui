(function(){
  // Lightweight client-side search post-processing.
  // If DocFX changes its global search object, wrap in try/catch.
  const STOPWORDS_URL = 'search-stopwords.json';
  const POLL_INTERVAL_MS = 250;
  const MAX_POLLS = 20;

  let stopwords = new Set();

  function loadStopwords(){
    return fetch(STOPWORDS_URL)
      .then(r => r.ok ? r.json() : [])
      .then(list => { stopwords = new Set(list.map(w => w.toLowerCase())); })
      .catch(()=>{});
  }

  function normalizeTerm(term){
    return term.toLowerCase().replace(/[^a-z0-9_\-\.]+/g,'');
  }

  function filterTerms(terms){
    return terms.filter(t => {
      const n = normalizeTerm(t);
      if(!n) return false;
      if(stopwords.has(n)) return false;
      return true;
    });
  }

  function enhanceSearch(){
    if(!window || !window.searchWorker){
      return false;
    }
    try {
      // Monkey patch the search UI input handling if present.
      const input = document.querySelector('input[type="search"], #search-query');
      if(!input) return false;

      if(input._tgxamlEnhanced) return true;
      input._tgxamlEnhanced = true;

      const originalHandler = input.oninput || function(){};
      input.addEventListener('input', function(e){
        const raw = e.target.value.trim();
        const split = raw.split(/\s+/);
        const filtered = filterTerms(split);
        if(filtered.length === 0 && split.length > 0){
          // If user only typed stopwords, keep original to avoid empty UI; else use filtered.
          // Fallback to original value to show no results gracefully.
        } else if(filtered.length !== split.length){
          // Reconstruct a cleaned query for worker consumption if search library reads from input.value.
          // We don't overwrite visible value to avoid confusing the user; instead we store on dataset.
          input.dataset.cleanedQuery = filtered.join(' ');
        } else {
          input.dataset.cleanedQuery = raw;
        }
        originalHandler.call(input, e);
      });

      console.log('[Terminal.Gui.Xaml] Search enhancements active (stopwords loaded: ' + stopwords.size + ')');
      return true;
    } catch(err){
      console.warn('[Terminal.Gui.Xaml] Search enhancement failed', err);
      return false;
    }
  }

  async function init(){
    await loadStopwords();
    let attempts = 0;
    const timer = setInterval(()=>{
      attempts++;
      if(enhanceSearch() || attempts >= MAX_POLLS){
        clearInterval(timer);
      }
    }, POLL_INTERVAL_MS);
  }

  if(document.readyState === 'loading'){
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
